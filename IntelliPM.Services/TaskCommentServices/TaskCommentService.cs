using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Services.SubtaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskCommentServices
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IMapper _mapper;
        private readonly ITaskCommentRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly ILogger<TaskCommentService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IProjectRepository _projectRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public TaskCommentService(IMapper mapper, ITaskCommentRepository repo, INotificationRepository notificationRepo, IProjectMemberRepository projectMemberRepo, ITaskRepository taskRepo, IActivityLogService activityLogService, IEmailService emailService, IAccountRepository accountRepository, IProjectRepository projectRepo, ILogger<TaskCommentService> logger, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _notificationRepo = notificationRepo;
            _projectMemberRepo = projectMemberRepo;
            _taskRepo = taskRepo;
            _activityLogService = activityLogService;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _projectRepo = projectRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }

        public async Task<TaskCommentResponseDTO> CreateTaskComment(TaskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Task comment content is required.", nameof(request.Content));

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK_COMMENT");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "CREATE");
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", "TASK_COMMENT_CREATE");
            var dynamicNotificationPriority = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_priority", "NORMAL");

            var entity = _mapper.Map<TaskComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                // Lưu comment
                await _repo.Add(entity);

                // Lấy task và kiểm tra
                var task = await _taskRepo.GetByIdAsync(request.TaskId);
                if (task == null)
                    throw new Exception($"Task with ID {request.TaskId} not found.");
                var projectId = task.ProjectId;

                var project = await _projectRepo.GetByIdAsync(projectId);
                if (project == null)
                    throw new Exception($"Project with ID {projectId} not found");
                // Ghi log
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    TaskId = entity.TaskId,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.TaskId,
                    ActionType = dynamicActionType,
                    Message = $"Comment in task '{entity.TaskId}' is '{request.Content}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });

                // Lấy danh sách thành viên dự án (trừ người tạo)
                var members = await _projectMemberRepo.GetProjectMemberbyProjectId(projectId);
                var recipients = members
                    .Where(m => m.AccountId != request.AccountId)
                    .Select(m => m.AccountId)
                    .ToList();

                // Gửi thông báo + email
                if (recipients.Count > 0)
                {
                    var notification = new Notification
                    {
                        CreatedBy = request.AccountId,
                        Type = dynamicNotificationType,
                        Priority = dynamicNotificationPriority,
                        Message = $"Comment in project {project.ProjectKey} - task {request.TaskId}: {request.Content}",
                        RelatedEntityType = dynamicEntityType,
                        RelatedEntityId = entity.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };

                    var taskTitle = task.Title ?? $"Task {task.Id}";
                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId,
                            IsRead = false
                        });

                        //var account = await _accountRepository.GetAccountById(accId);
                        //if (account != null && !string.IsNullOrEmpty(account.Email))
                        //{
                        //    await _emailService.SendTaskCommentNotificationEmail(account.Email, account.FullName, entity.TaskId, taskTitle, request.Content);
                        //}
                    }

                    await _notificationRepo.Add(notification);
                }
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task comment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }

        public async Task DeleteTaskComment(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task comment with ID {id} not found.");

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK_COMMENT");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "DELETE");

            try
            {
                await _repo.Delete(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.TaskId))?.ProjectId ?? 0,
                    TaskId = entity.TaskId,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.TaskId,
                    ActionType = dynamicActionType,
                    Message = $"Delete comment in task '{entity.TaskId}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task comment: {ex.Message}", ex);
            }
        }

        public async Task<List<TaskCommentResponseDTO>> GetAllTaskComment()
        {
            var entities = await _repo.GetAllTaskComment();
            return _mapper.Map<List<TaskCommentResponseDTO>>(entities);
        }

        public async Task<TaskCommentResponseDTO> GetTaskCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task comment with ID {id} not found.");

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }

        public async Task<TaskCommentResponseDTO> UpdateTaskComment(int id, TaskCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task comment with ID {id} not found.");

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK_COMMENT");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.TaskId))?.ProjectId ?? 0,
                    TaskId = entity.TaskId,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.TaskId,
                    ActionType = dynamicActionType,
                    Message = $"Update comment in task '{entity.TaskId}' is '{entity.Content}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task comment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }

        public async Task<List<TaskCommentResponseDTO>> GetTaskCommentByTaskIdAsync(string taskId)
        {
            {
                var entities = await _repo.GetTaskCommentByTaskIdAsync(taskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No subtasks found for Task ID {taskId}.");

                return _mapper.Map<List<TaskCommentResponseDTO>>(entities);
            }
        }
    }
}
