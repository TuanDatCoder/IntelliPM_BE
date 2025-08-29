using AutoMapper;
using IntelliPM.Data.DTOs.SubtaskComment.Request;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ActivityLogActionType;
using IntelliPM.Data.Enum.ActivityLogRelatedEntityType;
using IntelliPM.Data.Enum.Notification;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskCommentRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskCommentServices
{
    public class SubtaskCommentService : ISubtaskCommentService
    {
        private readonly IMapper _mapper;
        private readonly ISubtaskCommentRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly ILogger<SubtaskCommentService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ITaskRepository _taskRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IProjectRepository _projectRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public SubtaskCommentService(IMapper mapper, ISubtaskCommentRepository repo, INotificationRepository notificationRepo, IProjectMemberRepository projectMemberRepo, ISubtaskRepository subtaskRepo, ITaskRepository taskRepo, IActivityLogService activityLogService, IAccountRepository accountRepository, IEmailService emailService, IProjectRepository projectRepo, ILogger<SubtaskCommentService> logger, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _notificationRepo = notificationRepo;   
            _subtaskRepo = subtaskRepo;
            _projectMemberRepo = projectMemberRepo;
            _activityLogService = activityLogService;
            _taskRepo = taskRepo;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _projectRepo = projectRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }
        public async Task<SubtaskCommentResponseDTO> CreateSubtaskComment(SubtaskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Task comment content is required.", nameof(request.Content));

            var entity = _mapper.Map<SubtaskComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);

                var subtask = await _subtaskRepo.GetByIdAsync(request.SubtaskId);
                Console.WriteLine($"Creating comment for SubtaskId: {request.SubtaskId}");
                if (subtask == null)
                    throw new Exception($"Subtask with ID {request.SubtaskId} not found.");

                var projectId = subtask.Task.ProjectId;
                var project = await _projectRepo.GetByIdAsync(projectId);
                if (project == null)
                    throw new Exception($"Project with ID {projectId} not found");

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    TaskId = (await _subtaskRepo.GetByIdAsync(entity.SubtaskId))?.TaskId ?? null,
                    SubtaskId = entity.SubtaskId,
                    RelatedEntityType = ActivityLogRelatedEntityTypeEnum.SUBTASK_COMMENT.ToString(),
                    RelatedEntityId = entity.SubtaskId,
                    ActionType = ActivityLogActionTypeEnum.CREATE.ToString(),
                    Message = $"Comment in subtask '{entity.SubtaskId}' is '{request.Content}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });

                var members = await _projectMemberRepo.GetProjectMemberbyProjectId(projectId);
                var recipients = members
                    .Where(m => m.AccountId != request.AccountId)
                    .Select(m => m.AccountId)
                    .ToList();

                if (recipients.Count > 0)
                {
                    var notification = new Notification
                    {
                        CreatedBy = request.AccountId,
                        Type = NotificationActionTypeEnum.SUBTASK_COMMENT_CREATE.ToString(),
                        Priority = NotificationPriorityEnum.NORMAL.ToString(),
                        Message = $"Comment in project {project.ProjectKey} - subtask {request.SubtaskId}: {request.Content}",
                        RelatedEntityType = NotificationRelatedEntityTypeEnum.SUBTASK_COMMENT.ToString(),
                        RelatedEntityId = entity.Id, 
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };

                    var subtaskTitle = subtask.Title ?? $"Subtask {subtask.Id}";
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
                        //    await _emailService.SendSubtaskCommentNotificationEmail(account.Email, account.FullName, entity.SubtaskId, subtaskTitle, request.Content);
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
            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task DeleteSubtaskComment(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task subtask comment with ID {id} not found.");
            var subtask = await _subtaskRepo.GetByIdAsync(entity.SubtaskId);
            var projectId = subtask?.Task.ProjectId;

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "SUBTASK_COMMENT");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "DELETE");

            try
            {
                await _repo.Delete(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    TaskId = (await _subtaskRepo.GetByIdAsync(entity.SubtaskId))?.TaskId ?? null,
                    SubtaskId = entity.SubtaskId,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.SubtaskId,
                    ActionType = dynamicActionType,
                    Message = $"Delete comment in subtask '{entity.SubtaskId}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete subtask comment: {ex.Message}", ex);
            }
        }
        public async Task<List<SubtaskCommentResponseDTO>> GetAllSubtaskComment()
        {
            var entities = await _repo.GetAllSubtaskComment();
            return _mapper.Map<List<SubtaskCommentResponseDTO>>(entities);
        }

        public async Task<SubtaskCommentResponseDTO> GetSubtaskCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask comment with ID {id} not found.");

            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task<SubtaskCommentResponseDTO> UpdateSubtaskComment(int id, SubtaskCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask comment with ID {id} not found.");

            _mapper.Map(request, entity);

            var subtask = await _subtaskRepo.GetByIdAsync(request.SubtaskId);
            var projectId = subtask?.Task.ProjectId;

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "SUBTASK_COMMENT");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            try
            {
                await _repo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    TaskId = (await _subtaskRepo.GetByIdAsync(entity.SubtaskId))?.TaskId ?? null,
                    SubtaskId = entity.SubtaskId,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.SubtaskId,
                    ActionType = dynamicActionType,
                    Message = $"Update comment in subtask '{entity.SubtaskId}' is '{request.Content}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update subtask comment: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task<List<SubtaskCommentResponseDTO>> GetSubtaskCommentBySubtaskIdAsync(string subtaskId)
        {
            {
                var entities = await _repo.GetSubtaskCommentBySubtaskIdAsync(subtaskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No subtasks found for Task ID {subtaskId}.");

                return _mapper.Map<List<SubtaskCommentResponseDTO>>(entities);
            }
        }
    }
}
