using AutoMapper;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.SubtaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public TaskCommentService(IMapper mapper, ITaskCommentRepository repo, INotificationRepository notificationRepo, IProjectMemberRepository projectMemberRepo, ITaskRepository taskRepo, ILogger<TaskCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _notificationRepo = notificationRepo;
            _projectMemberRepo = projectMemberRepo;
            _taskRepo = taskRepo;
        }

        public async Task<TaskCommentResponseDTO> CreateTaskComment(TaskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Task comment content is required.", nameof(request.Content));

            var entity = _mapper.Map<TaskComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);

                var task = await _taskRepo.GetByIdAsync(request.TaskId);
                Console.WriteLine($"Creating comment for TaskId: {request.TaskId}");
                if (task == null)
                    throw new Exception($"Task with ID {request.TaskId} not found.");

                var projectId = task.ProjectId;

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
                        Type = "COMMENT",
                        Priority = "NORMAL",
                        Message = $"Đã bình luận trên task {request.TaskId}: {request.Content}",
                        RelatedEntityType = "Task",
                        RelatedEntityId = entity.Id, 
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };

                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId
                            //IsRead = false
                        });
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

        public async Task DeleteTaskComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task task comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
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

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
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
