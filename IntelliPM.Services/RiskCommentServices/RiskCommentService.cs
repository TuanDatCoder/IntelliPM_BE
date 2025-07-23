using AutoMapper;
using IntelliPM.Data.DTOs.RiskComment.Request;
using IntelliPM.Data.DTOs.RiskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.RiskCommentRepos;
using IntelliPM.Repositories.RiskRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskCommentServices
{
    public class RiskCommentService : IRiskCommentService
    {
        private readonly IMapper _mapper;
        private readonly IRiskCommentRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IRiskRepository _riskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly ILogger<RiskCommentService> _logger;

        public RiskCommentService(IMapper mapper, IRiskCommentRepository repo, INotificationRepository notificationRepo, IRiskRepository riskRepo, IProjectMemberRepository projectMemberRepo, ILogger<RiskCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _notificationRepo = notificationRepo;
            _riskRepo = riskRepo;
            _projectMemberRepo = projectMemberRepo;
            _logger = logger;
        }

        public async Task<RiskCommentResponseDTO> CreateRiskComment(RiskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Comment))
                throw new ArgumentException("Risk comment is required.", nameof(request.Comment));


            var entity = _mapper.Map<RiskComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);

                var risk = await _riskRepo.GetByIdAsync(request.RiskId);
                Console.WriteLine($"Creating comment for RiskId: {request.RiskId}");
                if (risk == null)
                    throw new Exception($"Risk with ID {request.RiskId} not found.");

                var projectId = risk.ProjectId;

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
                        Message = $"Comment in risk {risk.RiskKey}: {request.Comment}",
                        RelatedEntityType = "Risk",
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
                throw new Exception($"Failed to create risk comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create risk comment: {ex.Message}", ex);
            }
            return _mapper.Map<RiskCommentResponseDTO>(entity);
        }

        public async Task DeleteRiskComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete risk comment: {ex.Message}", ex);
            }
        }

        public async Task<List<RiskCommentResponseDTO>> GetAllRiskComment()
        {
            var entities = await _repo.GetAllRiskComment();
            return _mapper.Map<List<RiskCommentResponseDTO>>(entities);
        }

        public async Task<RiskCommentResponseDTO> GetById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk comment with ID {id} not found.");

            return _mapper.Map<RiskCommentResponseDTO>(entity);
        }

        public async Task<RiskCommentResponseDTO> UpdateRiskComment(int id, RiskCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Risk comment with ID {id} not found.");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk comment: {ex.Message}", ex);
            }

            return _mapper.Map<RiskCommentResponseDTO>(entity);
        }

        public async Task<List<RiskCommentResponseDTO>> GetByRiskIdAsync(int riskId)
        {
            {
                var entities = await _repo.GetByRiskIdAsync(riskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No risk comment found for Risk ID {riskId}.");

                return _mapper.Map<List<RiskCommentResponseDTO>>(entities);
            }
        }
    }
}
