using AutoMapper;
using IntelliPM.Data.DTOs.MilestoneComment.Request;
using IntelliPM.Data.DTOs.MilestoneComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.MilestoneCommentRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Services.EmailServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.MilestoneCommentServices
{
    public class MilestoneCommentService : IMilestoneCommentService
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneCommentRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<MilestoneCommentService> _logger;

        public MilestoneCommentService(
            IMapper mapper,
            IMilestoneCommentRepository repo,
            INotificationRepository notificationRepo,
            IMilestoneRepository milestoneRepo,
            IProjectMemberRepository projectMemberRepo,
            IAccountRepository accountRepository,
            IEmailService emailService,
            ILogger<MilestoneCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _notificationRepo = notificationRepo;
            _milestoneRepo = milestoneRepo;
            _projectMemberRepo = projectMemberRepo;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<MilestoneCommentResponseDTO> CreateMilestoneComment(MilestoneCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Milestone comment content is required.", nameof(request.Content));

            var account = await _projectMemberRepo.GetAccountByIdAsync(request.AccountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");

            var entity = _mapper.Map<MilestoneComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);

                var milestone = await _milestoneRepo.GetByIdAsync(request.MilestoneId);
                if (milestone == null)
                    throw new Exception($"Milestone with ID {request.MilestoneId} not found.");

                var projectId = milestone.ProjectId;
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
                        Message = $"Comment in milestone {request.MilestoneId}: {request.Content}",
                        RelatedEntityType = "Milestone",
                        RelatedEntityId = entity.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };
                    var milestoneTitle = milestone.Name ?? $"Milestone {milestone.Id}";
                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId
                        });
                    }

                }

                return _mapper.Map<MilestoneCommentResponseDTO>(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create milestone comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create milestone comment: {ex.Message}", ex);
            }
        }

        public async Task DeleteMilestoneComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete milestone comment: {ex.Message}", ex);
            }
        }

        public async Task<List<MilestoneCommentResponseDTO>> GetAllMilestoneComment(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) throw new ArgumentException("Invalid page or page size");
            var entities = (await _repo.GetAllMilestoneComment())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<MilestoneCommentResponseDTO>>(entities);
        }

        public async Task<MilestoneCommentResponseDTO> GetMilestoneCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone comment with ID {id} not found.");

            return _mapper.Map<MilestoneCommentResponseDTO>(entity);
        }

        public async Task<List<MilestoneCommentResponseDTO>> GetMilestoneCommentByMilestoneIdAsync(int milestoneId)
        {
            var entities = await _repo.GetMilestoneCommentByMilestoneIdAsync(milestoneId);
            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No comments found for Milestone ID {milestoneId}.");

            return _mapper.Map<List<MilestoneCommentResponseDTO>>(entities);
        }

        public async Task<MilestoneCommentResponseDTO> UpdateMilestoneComment(int id, MilestoneCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone comment with ID {id} not found.");

            var account = await _projectMemberRepo.GetAccountByIdAsync(request.AccountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");

            _mapper.Map(request, entity);
            entity.CreatedAt = DateTime.UtcNow; // Note: Consider using UpdatedAt if applicable

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update milestone comment: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneCommentResponseDTO>(entity);
        }
    }
}
