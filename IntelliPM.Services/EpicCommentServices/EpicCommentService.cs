using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.EpicComment.Request;
using IntelliPM.Data.DTOs.EpicComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicCommentRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
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

namespace IntelliPM.Services.EpicCommentServices
{
    public class EpicCommentService : IEpicCommentService
    {
        private readonly IMapper _mapper;
        private readonly IEpicCommentRepository _repo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly ILogger<EpicCommentService> _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IActivityLogService _activityLogService;
        private readonly IProjectRepository _projectRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;
        public EpicCommentService(IMapper mapper, IEpicCommentRepository repo, INotificationRepository notificationRepo, IEpicRepository epicRepo, IProjectMemberRepository projectMemberRepo, IAccountRepository accountRepository, IActivityLogService activityLogService, IEmailService emailService, ILogger<EpicCommentService> logger, IProjectRepository projectRepo, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _notificationRepo = notificationRepo;
            _epicRepo = epicRepo;
            _projectMemberRepo = projectMemberRepo;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _activityLogService = activityLogService;
            _projectRepo = projectRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }


        public async Task<EpicCommentResponseDTO> CreateEpicComment(EpicCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Epic comment content is required.", nameof(request.Content));

            var account = await _projectMemberRepo.GetAccountByIdAsync(request.AccountId); 
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");

            var entity = _mapper.Map<EpicComment>(request);
            entity.CreatedAt = DateTime.UtcNow;

            var createActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "CREATE");
            var dynamicRelatedEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "EPICCOMMENT");
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", "EPICCOMMENT_CREATE");
            var dynamicNotificationPriority = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_priority", "NORMAL");

            try
            {
                var epic = await _epicRepo.GetByIdAsync(request.EpicId);
                if (epic == null)
                    throw new Exception($"Epic with ID {request.EpicId} not found.");

                var projectId = epic.ProjectId;

                await _repo.Add(entity);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    EpicId = entity.EpicId,
                    RelatedEntityType = dynamicRelatedEntityType,
                    RelatedEntityId = entity.EpicId,
                    ActionType = createActionType,
                    Message = $"Create comment in epic '{entity.EpicId}' is '{request.Content}'",
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
                        Type = dynamicNotificationType,
                        Priority = "NORMAL",
                        Message = $"Comment in epic {request.EpicId}: {request.Content}",
                        RelatedEntityType = dynamicRelatedEntityType,
                        RelatedEntityId = entity.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };
                    var epicTitle = epic.Name ?? $"Epic {epic.Id}";
                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId
                        });
                    }
                    
                    //if (account != null && !string.IsNullOrEmpty(account.Email))
                    //{
                    //    await _emailService.SendEpicCommentNotificationEmail(account.Email, account.FullName, entity.EpicId, epicTitle, request.Content);
                    //}
                    await _notificationRepo.Add(notification);
                }
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create epic comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create epic comment: {ex.Message}", ex);
            }
            return _mapper.Map<EpicCommentResponseDTO>(entity);
        }

        public async Task DeleteEpicComment(int id, int createdBy)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic comment with ID {id} not found.");

            var deleteActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "DELETE");
            var dynamicRelatedEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "EPICCOMMENT");

            try
            {
                var epic = await _epicRepo.GetByIdAsync(entity.EpicId);
                if (epic == null)
                    throw new Exception($"Epic with ID {entity.EpicId} not found.");

                var projectId = epic.ProjectId;

                await _repo.Delete(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    EpicId = entity.EpicId,
                    RelatedEntityType = dynamicRelatedEntityType,
                    RelatedEntityId = entity.EpicId,
                    ActionType = deleteActionType,
                    Message = $"Delete comment in epic '{entity.EpicId}' is '{entity.Content}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete epic comment: {ex.Message}", ex);
            }
        }

        public async Task<List<EpicCommentResponseDTO>> GetAllEpicComment(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) throw new ArgumentException("Invalid page or page size");
            var entities = (await _repo.GetAllEpicComment())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<EpicCommentResponseDTO>>(entities);
        }

        public async Task<List<EpicCommentResponseDTO>> GetEpicCommentByEpicIdAsync(string epicId)
        {
            {
                var entities = await _repo.GetEpicCommentByEpicIdAsync(epicId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No epics found for Task ID {epicId}.");

                return _mapper.Map<List<EpicCommentResponseDTO>>(entities);
            }
        }

        public async Task<EpicCommentResponseDTO> GetEpicCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic comment with ID {id} not found.");

            return _mapper.Map<EpicCommentResponseDTO>(entity);
        }

        public async Task<EpicCommentResponseDTO> UpdateEpicComment(int id, EpicCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic comment with ID {id} not found.");

            var account = await _projectMemberRepo.GetAccountByIdAsync(request.AccountId);
            if (account == null)
                throw new KeyNotFoundException($"Account with ID {request.AccountId} not found.");

            _mapper.Map(request, entity);
            entity.CreatedAt = DateTime.UtcNow;

            var updateActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");
            var dynamicRelatedEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "EPICCOMMENT");

            try
            {
                var epic = await _epicRepo.GetByIdAsync(entity.EpicId);
                if (epic == null)
                    throw new Exception($"Epic with ID {entity.EpicId} not found.");

                var projectId = epic.ProjectId;

                await _repo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = projectId,
                    EpicId = entity.EpicId,
                    RelatedEntityType = dynamicRelatedEntityType,
                    RelatedEntityId = entity.EpicId,
                    ActionType = updateActionType,
                    Message = $"Update comment in epic '{entity.EpicId}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update epic comment: {ex.Message}", ex);
            }

            return _mapper.Map<EpicCommentResponseDTO>(entity);
        }



    }
}
