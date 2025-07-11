﻿using AutoMapper;
using IntelliPM.Data.DTOs.SubtaskComment.Request;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.SubtaskCommentRepos;
using IntelliPM.Repositories.SubtaskRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public SubtaskCommentService(IMapper mapper, ISubtaskCommentRepository repo, INotificationRepository notificationRepo, IProjectMemberRepository projectMemberRepo, ISubtaskRepository subtaskRepo, ILogger<SubtaskCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _notificationRepo = notificationRepo;   
            _subtaskRepo = subtaskRepo;
            _projectMemberRepo = projectMemberRepo;
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
                Console.WriteLine($"Creating comment for TaskId: {request.SubtaskId}");
                if (subtask == null)
                    throw new Exception($"Task with ID {request.SubtaskId} not found.");

                var projectId = subtask.Task.ProjectId;

                // 👥 2. Lấy danh sách thành viên dự án (trừ người đang comment)
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
                        Message = $"Đã bình luận trên subtask {request.SubtaskId}: {request.Content}",
                        RelatedEntityType = "Subtask",
                        RelatedEntityId = entity.Id, // comment ID
                        CreatedAt = DateTime.UtcNow,
                        //IsRead = false,
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
            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task DeleteSubtaskComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task subtask comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
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

            try
            {
                await _repo.Update(entity);
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
