using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ActivityLogActionType;
using IntelliPM.Data.Enum.ActivityLogRelatedEntityType;
using IntelliPM.Data.Enum.Notification;
using IntelliPM.Data.Enum.Risk;
using IntelliPM.Data.Enum.Task;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
using IntelliPM.Repositories.SystemConfigurationRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IntelliPM.Services.RiskServices
{
    public class RiskService : IRiskService
    {
        private readonly IRiskRepository _riskRepo;
        private readonly IRiskSolutionRepository _riskSolutionRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IGeminiService _geminiService;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationRepository _notificationRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly ISystemConfigurationRepository _systemConfigRepo;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public RiskService(IRiskRepository riskRepo, IRiskSolutionRepository riskSolutionRepo, IGeminiService geminiService, ITaskRepository taskRepo, IProjectRepository projectRepo, IProjectMemberRepository projectMemberRepo, IMapper mapper, IActivityLogService activityLogService, INotificationRepository notificationRepo, IAccountRepository accountRepo, IEmailService emailService, IConfiguration config, ITaskAssignmentRepository taskAssignmentRepo, ISystemConfigurationRepository systemConfigRepo, IDynamicCategoryRepository dynamicCategoryRepo, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _riskRepo = riskRepo;
            _riskSolutionRepo = riskSolutionRepo;
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
            _projectMemberRepo = projectMemberRepo;
            _geminiService = geminiService;
            _activityLogService = activityLogService;
            _notificationRepo = notificationRepo;
            _accountRepo = accountRepo;
            _emailService = emailService;
            _mapper = mapper;
            _config = config;
            _taskAssignmentRepo = taskAssignmentRepo;
            _systemConfigRepo = systemConfigRepo;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }

        public async Task<List<RiskResponseDTO>> GetAllRisksAsync()
        {
            var risks = await _riskRepo.GetAllRisksAsync();
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<List<RiskResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            var risks = await _riskRepo.GetByProjectIdAsync(projectId);
            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        public async Task<RiskResponseDTO> GetByIdAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task DeleteAsync(int id)
        {
            var risk = await _riskRepo.GetByIdAsync(id)
                ?? throw new Exception("Risk not found");
            await _riskRepo.DeleteAsync(risk);
        }

        public async Task<List<RiskResponseDTO>> GetByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var risks = await _riskRepo.GetByProjectIdAsync(project.Id);

            return _mapper.Map<List<RiskResponseDTO>>(risks);
        }

        //public async Task<RiskResponseDTO> CreateRiskAsync(RiskCreateRequestDTO request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Title))
        //        throw new ArgumentException("Title is required.");
        //    if (string.IsNullOrWhiteSpace(request.RiskScope))
        //        throw new ArgumentException("RiskScope is required.");
        //    if (string.IsNullOrWhiteSpace(request.ProjectKey))
        //        throw new ArgumentException("ProjectKey is required.");

        //    var project = await _projectRepo.GetProjectByKeyAsync(request.ProjectKey)
        //        ?? throw new Exception("Project not found with provided projectKey");

        //    var count = await _riskRepo.CountByProjectIdAsync(project.Id);
        //    var nextIndex = count + 1;
        //    var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

        //    var impactLevel = request.ImpactLevel;
        //    var probability = request.Probability;

        //    var entity = _mapper.Map<Risk>(request);
        //    entity.ProjectId = project.Id;
        //    entity.RiskKey = riskKey;
        //    entity.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);
        //    entity.CreatedAt = DateTime.UtcNow;
        //    entity.UpdatedAt = DateTime.UtcNow;

        //    await _riskRepo.AddAsync(entity);

        //    // Construct riskDetailUrl using configuration
        //    var baseUrl = _config["Environment:FE_URL"];
        //    var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{riskKey}";

        //    if (request.ResponsibleId.HasValue)
        //    {
        //        var assignedUser = await _accountRepo.GetAccountById(request.ResponsibleId.Value);
        //        if (assignedUser != null)
        //        {
        //            await _emailService.SendRiskAssignmentEmail(
        //                assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
        //                assigneeEmail: assignedUser.Email,
        //                riskKey: riskKey,
        //                riskTitle: request.Title,
        //                projectKey: project.ProjectKey,
        //                severityLevel: entity.SeverityLevel,
        //                dueDate: request.DueDate,
        //                riskDetailUrl: riskDetailUrl
        //            );
        //        }
        //    }

        //    await _activityLogService.LogAsync(new ActivityLog
        //    {
        //        ProjectId = entity.ProjectId,
        //        RiskKey = riskKey,
        //        RelatedEntityType = "Risk",
        //        RelatedEntityId = riskKey,
        //        ActionType = "CREATE",
        //        Message = $"Created risk '{riskKey}'",
        //        CreatedBy = request.CreatedBy,
        //        CreatedAt = DateTime.UtcNow
        //    });

        //    var members = await _projectMemberRepo.GetProjectMemberbyProjectId(entity.ProjectId);
        //    var recipients = members
        //        .Where(m => m.AccountId != request.CreatedBy)
        //        .Select(m => m.AccountId)
        //        .ToList();

        //    if (recipients.Count > 0)
        //    {
        //        var notification = new Notification
        //        {
        //            CreatedBy = request.CreatedBy,
        //            Type = "RISK_ALERT",
        //            Priority = entity.SeverityLevel,
        //            Message = $"Risk identified in project {project.ProjectKey} - risk {riskKey}",
        //            RelatedEntityType = "Risk",
        //            RelatedEntityId = entity.Id,
        //            CreatedAt = DateTime.UtcNow,
        //            IsRead = false,
        //            RecipientNotification = new List<RecipientNotification>()
        //        };

        //        foreach (var accId in recipients)
        //        {
        //            notification.RecipientNotification.Add(new RecipientNotification
        //            {
        //                AccountId = accId,
        //                IsRead = false
        //            });
        //        }
        //        await _notificationRepo.Add(notification);
        //    }

        //    return _mapper.Map<RiskResponseDTO>(entity);
        //}

        public async Task<RiskResponseDTO> CreateRiskAsync(RiskCreateRequestDTO request)
        {
            // Validate request using DataAnnotations
            var context = new ValidationContext(request);
            Validator.ValidateObject(request, context, validateAllProperties: true);

            var project = await _projectRepo.GetProjectByKeyAsync(request.ProjectKey)
                ?? throw new Exception("Project not found with provided projectKey");

            if (request.RiskScope == RiskScopeEnum.TASK.ToString() && request.TaskId == null)
            {
                throw new Exception("TaskId is required when risk scope is TASK");
            }
            if (request.TaskId != null)
            {
                var task = await _taskRepo.GetByIdAsync(request.TaskId);
                if (task == null || task.ProjectId != project.Id)
                {
                    throw new Exception("Invalid or non-existent TaskId for the specified project");
                }
            }

            // Fetch default values from system_configuration
            var defaultStatusConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_status");
            var defaultGeneratedByConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_generated_by");

            var defaultStatus = defaultStatusConfig?.ValueConfig ?? "OPEN";
            var defaultGeneratedBy = defaultGeneratedByConfig?.ValueConfig ?? "MANUAL";
            var defaultRiskScope = (await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync(RiskScopeEnum.PROJECT.ToString(), "risk_scope"))
                ?.FirstOrDefault()?.Name ?? "PROJECT";
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.CREATE.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

            var count = await _riskRepo.CountByProjectIdAsync(project.Id);
            var nextIndex = count + 1;
            var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

            var impactLevel = request.ImpactLevel;
            var probability = request.Probability;

            var entity = _mapper.Map<Risk>(request);
            entity.ProjectId = project.Id;
            entity.RiskKey = riskKey;
            entity.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);
            entity.Status = request.Status ?? defaultStatus;
            entity.GeneratedBy = request.GeneratedBy ?? defaultGeneratedBy;
            entity.RiskScope = request.RiskScope ?? defaultRiskScope;
            entity.TaskId = request.TaskId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _riskRepo.AddAsync(entity);

            // Construct riskDetailUrl using configuration
            var baseUrl = _config["Environment:FE_URL"];
            var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{riskKey}";

            if (request.ResponsibleId.HasValue)
            {
                var assignedUser = await _accountRepo.GetAccountById(request.ResponsibleId.Value);
                if (assignedUser != null)
                {
                    await _emailService.SendRiskAssignmentEmail(
                        assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
                        assigneeEmail: assignedUser.Email,
                        riskKey: riskKey,
                        riskTitle: request.Title,
                        projectKey: project.ProjectKey,
                        severityLevel: entity.SeverityLevel,
                        dueDate: request.DueDate,
                        riskDetailUrl: riskDetailUrl
                    );
                }
            }

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = entity.ProjectId,
                RiskKey = riskKey,
                RelatedEntityType = dynamicEntityType,
                RelatedEntityId = riskKey,
                ActionType = dynamicActionType,
                Message = $"Created risk '{riskKey}'",
                CreatedBy = (int)request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            });

            var members = await _projectMemberRepo.GetProjectMemberbyProjectId(entity.ProjectId);
            var recipients = members
                .Where(m => m.AccountId != request.CreatedBy)
                .Select(m => m.AccountId)
                .ToList();

            if (recipients.Count > 0)
            {
                var notification = new Notification
                {
                    CreatedBy = (int)request.CreatedBy,
                    Type = dynamicNotificationType,
                    Priority = entity.SeverityLevel,
                    Message = $"Risk identified in project {project.ProjectKey} - risk {riskKey}",
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    RecipientNotification = new List<RecipientNotification>()
                };

                foreach (var accId in recipients)
                {
                    notification.RecipientNotification.Add(new RecipientNotification
                    {
                        AccountId = accId,
                        IsRead = false
                    });
                }
                await _notificationRepo.Add(notification);
            }

            return _mapper.Map<RiskResponseDTO>(entity);
        }

        public async Task<RiskResponseDTO?> UpdateStatusAsync(int id, string status, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var project = await _projectRepo.GetByIdAsync(risk.ProjectId)
                ?? throw new Exception("Project not found with provided projectId");

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());

            risk.Status = status;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);

                var members = await _projectMemberRepo.GetProjectMemberbyProjectId(risk.ProjectId);
                var recipients = members
                    .Where(m => m.AccountId != createdBy)
                    .Select(m => m.AccountId)
                    .ToList();

                if (recipients.Count > 0)
                {
                    var notification = new Notification
                    {
                        CreatedBy = createdBy,
                        Type = dynamicNotificationType,
                        Priority = risk.SeverityLevel,
                        Message = $"Updated status in risk in project {project.ProjectKey} - risk {risk.RiskKey}: {status}",
                        RelatedEntityType = dynamicEntityType,
                        RelatedEntityId = risk.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };

                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId,
                            IsRead = false
                        });
                    }
                    await _notificationRepo.Add(notification);
                }

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated status in risk '{risk.RiskKey}' to '{status}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk status: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateTypeAsync(int id, string type, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());

            risk.Type = type;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated type in risk '{risk.RiskKey}' to '{type}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk type: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateResponsibleIdAsync(int id, int? responsibleId, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var project = await _projectRepo.GetByIdAsync(risk.ProjectId)
                ?? throw new Exception("Project not found with provided projectId");
            if (responsibleId.HasValue)
            {
                var user = await _accountRepo.GetAccountById(responsibleId.Value);
                if (user == null)
                    throw new ArgumentException("Responsible user not found");
            }

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

            risk.ResponsibleId = responsibleId;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
                var baseUrl = _config["Environment:FE_URL"];
                var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{risk.RiskKey}";

                var members = await _projectMemberRepo.GetProjectMemberbyProjectId(risk.ProjectId);
                var recipients = members
                    .Where(m => m.AccountId != createdBy)
                    .Select(m => m.AccountId)
                    .ToList();

                if (recipients.Count > 0)
                {
                    var notification = new Notification
                    {
                        CreatedBy = createdBy,
                        Type = dynamicNotificationType,
                        Priority = risk.SeverityLevel,
                        Message = $"Updated responsible person in project {project.ProjectKey} - risk {risk.RiskKey}",
                        RelatedEntityType = dynamicEntityType,
                        RelatedEntityId = risk.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        RecipientNotification = new List<RecipientNotification>()
                    };

                    foreach (var accId in recipients)
                    {
                        notification.RecipientNotification.Add(new RecipientNotification
                        {
                            AccountId = accId,
                            IsRead = false
                        });
                    }
                    await _notificationRepo.Add(notification);
                }

                if (responsibleId.HasValue)
                {
                    var assignedUser = await _accountRepo.GetAccountById(responsibleId.Value);
                    if (assignedUser != null)
                    {
                        await _emailService.SendRiskAssignmentEmail(
                            assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
                            assigneeEmail: assignedUser.Email,
                            riskKey: risk.RiskKey,
                            riskTitle: risk.Title,
                            projectKey: project.ProjectKey,
                            severityLevel: risk.SeverityLevel,
                            dueDate: risk.DueDate,
                            riskDetailUrl: riskDetailUrl
                        );
                    }
                }
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated responsible person in risk '{risk.RiskKey}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update risk responsible id: {ex.Message}", ex);
            }
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateDueDateAsync(int id, DateTime dueDate, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            if (dueDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Due date cannot be in the past");
            // Validate due date is not after project end date
            var project = await _projectRepo.GetByIdAsync(risk.ProjectId);
            if (project?.EndDate != null && dueDate.Date > project.EndDate.Value)
                throw new ArgumentException("Due date cannot be after project end date");

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());

            risk.DueDate = dueDate;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated due date in risk '{risk.RiskKey}' to {dueDate}",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk due date: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateTitleAsync(int id, string title, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Risk title is required");

            var maxLengthConfig = await _systemConfigRepo.GetByConfigKeyAsync("risk_title_length");
            int maxLength = maxLengthConfig != null ? int.Parse(maxLengthConfig.ValueConfig) : 200;
            if (title.Length > maxLength)
                throw new ArgumentException($"Title exceeds maximum length of {maxLength} characters");

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

            risk.Title = title;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated title in risk '{risk.RiskKey}' to '{title}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk title: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateDescriptionAsync(int id, string description, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

            risk.Description = description;
            risk.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated description in risk '{risk.RiskKey}' to '{description}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk description: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<RiskResponseDTO?> UpdateImpactLevelAsync(int id, string impactLevel, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
            var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

            risk.ImpactLevel = impactLevel;
            risk.UpdatedAt = DateTime.UtcNow;

            var probability = risk.Probability;
            risk.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated impact level in risk '{risk.RiskKey}' to '{impactLevel}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk impact level: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        private string CalculateSeverityLevel(string impact, string probability)
        {
            int GetLevelValue(string level)
            {
                return level.ToLower() switch
                {
                    "low" => 1,
                    "medium" => 2,
                    "high" => 3,
                    _ => 0
                };
            }

            var impactValue = GetLevelValue(impact);
            var probValue = GetLevelValue(probability);

            return (impactValue, probValue) switch
            {
                (1, 1) => "Low",
                (1, 2) => "Low",
                (1, 3) => "Medium",
                (2, 1) => "Low",
                (2, 2) => "Medium",
                (2, 3) => "High",
                (3, 1) => "Medium",
                (3, 2) => "High",
                (3, 3) => "High",
                _ => "Unknown"
            };
        }

        public async Task<RiskResponseDTO?> UpdateProbabilityAsync(int id, string probability, int createdBy)
        {
            var risk = await _riskRepo.GetByIdAsync(id);
            if (risk == null) return null;

            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());

            risk.Probability = probability;
            risk.UpdatedAt = DateTime.UtcNow;

            var impactLevel = risk.ImpactLevel;
            risk.SeverityLevel = CalculateSeverityLevel(impactLevel, probability);

            try
            {
                await _riskRepo.UpdateAsync(risk);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = risk.ProjectId,
                    RiskKey = risk.RiskKey,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = risk.RiskKey,
                    ActionType = dynamicActionType,
                    Message = $"Updated probability level in risk '{risk.RiskKey}' to '{probability}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change risk probability: {ex.Message}", ex);
            }

            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<List<AIRiskResponseDTO>> ViewAIProjectRisksAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var risks = await _geminiService.ViewAIProjectRisksAsync(project, tasks);

            return risks ?? new List<AIRiskResponseDTO>();
        }

        public async Task<RiskResponseDTO> GetByKeyAsync(string key)
        {
            var risk = await _riskRepo.GetByKeyAsync(key)
                ?? throw new Exception("Risk not found");
            return _mapper.Map<RiskResponseDTO>(risk);
        }

        public async Task<List<AIRiskResponseDTO>> ViewAIDetectTaskRisksAsyncAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var risks = await _geminiService.DetectTaskRisksAsync(project, tasks);

            return risks ?? new List<AIRiskResponseDTO>();
        }

        public async Task CheckAndCreateOverdueTaskRisksAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found with provided projectKey");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var now = DateTime.UtcNow.Date;
            var overdueTasks = tasks
                .Where(t => t.PlannedEndDate.HasValue &&
                            t.PlannedEndDate.Value < now &&
                            !string.Equals(t.Status, TaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var task in overdueTasks)
            {
                Console.WriteLine($"Processing task {task.Id}: Title={task.Title}, PlannedEndDate={task.PlannedEndDate}, Status={task.Status}");
                var existingRisk = await _riskRepo.GetRiskByTaskIdAsync(task.Id);
                if (existingRisk.Any())
                {
                    Console.WriteLine($"Risk already exists for task {task.Id}, skipping...");
                    continue;
                }

                var count = await _riskRepo.CountByProjectIdAsync(project.Id);
                var nextIndex = count + 1;
                var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

                // Fetch dynamic categories
                var riskScopes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_scope");
                var riskTypes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_type");
                var probabilities = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_probability_level");
                var impactLevels = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_impact_level");

                var defaultRiskScope = riskScopes.FirstOrDefault(c => c.Name == "TASK")?.Name ?? "TASK";
                var defaultRiskType = riskTypes.FirstOrDefault(c => c.Name == "SCHEDULE")?.Name ?? "SCHEDULE";
                var defaultProbability = probabilities.FirstOrDefault(c => c.Name == "HIGH")?.Name ?? "HIGH";
                var defaultImpactLevel = impactLevels.FirstOrDefault(c => c.Name == "MEDIUM")?.Name ?? "MEDIUM";

                var defaultStatusConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_status");
                var defaultStatus = defaultStatusConfig?.ValueConfig ?? "OPEN";

                var defaultGeneratedByConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_generated_by");
                var defaultGeneratedBy = defaultGeneratedByConfig?.ValueConfig ?? "SYSTEM";

                var dynamicRiskType = await _dynamicCategoryHelper.GetCategoryNameAsync("risk_type", "SCHEDULE");
                var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", ActivityLogActionTypeEnum.UPDATE.ToString());
                var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", ActivityLogRelatedEntityTypeEnum.RISK.ToString());
                var dynamicNotificationType = await _dynamicCategoryHelper.GetCategoryNameAsync("notification_type", NotificationRelatedEntityTypeEnum.RISK_ALERT.ToString());

                var entity = new Risk
                {
                    ProjectId = project.Id,
                    TaskId = task.Id,
                    RiskKey = riskKey,
                    RiskScope = defaultRiskScope,
                    Title = $"Overdue Task: {task.Id} - {task.Title}",
                    Description = $"Task {task.Id} is overdue. Planned end date was {task.PlannedEndDate:yyyy-MM-dd}.",
                    Status = defaultStatus,
                    Type = dynamicRiskType,
                    GeneratedBy = defaultGeneratedBy,
                    Probability = defaultProbability,
                    ImpactLevel = defaultImpactLevel,
                    SeverityLevel = CalculateSeverityLevel(defaultImpactLevel, defaultProbability),
                    CreatedBy = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = null,
                    IsApproved = true
                };

                await _riskRepo.AddAsync(entity);

                var baseUrl = _config["Environment:FE_URL"];
                var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{riskKey}";

                var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(task.Id);
                var emailTasks = taskAssignments
                    .Select(async taskAssignment =>
                    {
                        var assignedUser = await _accountRepo.GetAccountById(taskAssignment.AccountId);
                        if (assignedUser != null)
                        {
                            return _emailService.SendOverdueTaskNotificationEmailAsync(
                                assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
                                assigneeEmail: assignedUser.Email,
                                taskId: task.Id,
                                taskTitle: entity.Title,
                                projectKey: project.ProjectKey,
                                plannedEndDate: task.PlannedEndDate.Value,
                                taskDetailUrl: riskDetailUrl
                            );
                        }
                        return Task.CompletedTask;
                    })
                    .ToArray();

                await Task.WhenAll(emailTasks);

            }
        }

        //public async Task CheckAndCreateOverdueTaskRisksAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found with provided projectKey");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    var now = DateTime.UtcNow.Date;
        //    var overdueTasks = tasks
        //        .Where(t => t.PlannedEndDate.HasValue &&
        //            t.PlannedEndDate.Value < now &&
        //            !string.Equals(t.Status, "DONE", StringComparison.OrdinalIgnoreCase))
        //        .ToList();

        //    foreach (var task in overdueTasks)
        //    {
        //        Console.WriteLine($"Processing task {task.Id}: Title={task.Title}, PlannedEndDate={task.PlannedEndDate}, Status={task.Status}");
        //        // Check if risk already exists for this task
        //        var existingRisk = await _riskRepo.GetRiskByTaskIdAsync(task.Id);
        //        if (existingRisk.Any())
        //        {
        //            Console.WriteLine($"Risk already exists for task {task.Id}, skipping...");
        //            continue;
        //        }


        //        var count = await _riskRepo.CountByProjectIdAsync(project.Id);
        //        var nextIndex = count + 1;
        //        var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

        //        var entity = new Risk
        //        {
        //            ProjectId = project.Id,
        //            TaskId = task.Id,
        //            RiskKey = riskKey,
        //            RiskScope = "TASK",
        //            Title = $"Overdue Task: {task.Id} - {task.Title}",
        //            Description = $"Task {task.Id} is overdue. Planned end date was {task.PlannedEndDate:yyyy-MM-dd}.",
        //            Status = "OPEN",
        //            Type = "SCHEDULE",
        //            GeneratedBy = "SYSTEM",
        //            Probability = "HIGH",
        //            ImpactLevel = "MEDIUM",
        //            SeverityLevel = CalculateSeverityLevel("MEDIUM", "HIGH"),
        //            CreatedBy = 4,
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow,
        //            DueDate = null,
        //            IsApproved = true
        //        };

        //        await _riskRepo.AddAsync(entity);

        //        // Construct riskDetailUrl
        //        var baseUrl = _config["Environment:FE_URL"];
        //        var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{riskKey}";

        //        // Send email to responsible person if exists
        //        //var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(task.Id);
        //        //foreach (var taskAssignment in taskAssignments)
        //        //{
        //        //        var assignedUser = await _accountRepo.GetAccountById(taskAssignment.AccountId);
        //        //        if (assignedUser != null)
        //        //        {
        //        //            await _emailService.SendOverdueTaskNotificationEmailAsync(
        //        //                assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
        //        //                assigneeEmail: assignedUser.Email,
        //        //                taskId: task.Id,
        //        //                taskTitle: entity.Title,
        //        //                projectKey: project.ProjectKey,
        //        //                plannedEndDate: task.PlannedEndDate.Value,
        //        //                //dueDate: entity.DueDate,
        //        //                taskDetailUrl: riskDetailUrl
        //        //            );
        //        //        }
        //        //}

        //        // Async email sending
        //        var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(task.Id);
        //        var emailTasks = taskAssignments
        //            .Select(async taskAssignment =>
        //            {
        //                var assignedUser = await _accountRepo.GetAccountById(taskAssignment.AccountId);
        //                if (assignedUser != null)
        //                {
        //                    return _emailService.SendOverdueTaskNotificationEmailAsync(
        //                        assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
        //                        assigneeEmail: assignedUser.Email,
        //                        taskId: task.Id,
        //                        taskTitle: entity.Title,
        //                        projectKey: project.ProjectKey,
        //                        plannedEndDate: task.PlannedEndDate.Value,
        //                        taskDetailUrl: riskDetailUrl
        //                    );
        //                }
        //                return Task.CompletedTask;
        //            })
        //            .ToArray();

        //        await Task.WhenAll(emailTasks);

        //        // Notify project members
        //        var members = await _projectMemberRepo.GetProjectMemberbyProjectId(entity.ProjectId);
        //        var recipients = members
        //            .Where(m => m.AccountId != entity.CreatedBy)
        //            .Select(m => m.AccountId)
        //            .ToList();

        //        if (recipients.Count > 0)
        //        {
        //            var notification = new Notification
        //            {
        //                CreatedBy = entity.CreatedBy,
        //                Type = "RISK_ALERT",
        //                Priority = entity.SeverityLevel,
        //                Message = $"Overdue task risk identified in project {project.ProjectKey} - risk {riskKey}",
        //                RelatedEntityType = "Risk",
        //                RelatedEntityId = entity.Id,
        //                CreatedAt = DateTime.UtcNow,
        //                IsRead = false,
        //                RecipientNotification = new List<RecipientNotification>()
        //            };

        //            foreach (var accId in recipients)
        //            {
        //                notification.RecipientNotification.Add(new RecipientNotification
        //                {
        //                    AccountId = accId,
        //                    IsRead = false
        //                });
        //            }
        //            await _notificationRepo.Add(notification);
        //        }
        //    }
        //}

        public async Task CheckAndCreateOverdueTaskRisksForAllProjectsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var projects = await _projectRepo.GetAllProjects();

            foreach (var project in projects)
            {
                var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
                var overdueTasks = tasks
                    .Where(t => t.PlannedEndDate.HasValue &&
                                t.PlannedEndDate.Value < today &&
                                !string.Equals(t.Status, "DONE", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var task in overdueTasks)
                {
                    Console.WriteLine($"Processing task {task.Id}: Title={task.Title}, PlannedEndDate={task.PlannedEndDate}, Status={task.Status}");
                    var existingRisks = await _riskRepo.GetRiskByTaskIdAsync(task.Id);
                    if (existingRisks.Any())
                    {
                        Console.WriteLine($"Risk already exists for task {task.Id}, skipping...");
                        continue;
                    }

                    var count = await _riskRepo.CountByProjectIdAsync(project.Id);
                    var nextIndex = count + 1;
                    var riskKey = $"{project.ProjectKey}-R{nextIndex:D3}";

                    var riskScopes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_scope");
                    var riskTypes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_type");
                    var probabilities = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_probability_level");
                    var impactLevels = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_impact_level");

                    var defaultRiskScope = riskScopes.FirstOrDefault(c => c.Name == "TASK")?.Name ?? "TASK";
                    var defaultRiskType = riskTypes.FirstOrDefault(c => c.Name == "SCHEDULE")?.Name ?? "SCHEDULE";
                    var defaultProbability = probabilities.FirstOrDefault(c => c.Name == "HIGH")?.Name ?? "HIGH";
                    var defaultImpactLevel = impactLevels.FirstOrDefault(c => c.Name == "MEDIUM")?.Name ?? "MEDIUM";

                    var defaultStatusConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_status");
                    var defaultStatus = defaultStatusConfig?.ValueConfig ?? "OPEN";

                    var defaultGeneratedByConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_generated_by");
                    var defaultGeneratedBy = defaultGeneratedByConfig?.ValueConfig ?? "SYSTEM";

                    var dynamicRiskType = await _dynamicCategoryHelper.GetCategoryNameAsync("risk_type", "SCHEDULE");
                    var entity = new Risk
                    {
                        ProjectId = project.Id,
                        TaskId = task.Id,
                        RiskKey = riskKey,
                        RiskScope = defaultRiskScope,
                        Title = $"Overdue Task: {task.Id} - {task.Title}",
                        Description = $"Task {task.Id} is overdue. Planned end date was {task.PlannedEndDate:yyyy-MM-dd}.",
                        Status = defaultStatus,
                        Type = dynamicRiskType,
                        GeneratedBy = defaultGeneratedBy,
                        Probability = defaultProbability,
                        ImpactLevel = defaultImpactLevel,
                        SeverityLevel = CalculateSeverityLevel(defaultImpactLevel, defaultProbability),
                        CreatedBy = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        DueDate = null,
                        IsApproved = true
                    };

                    await _riskRepo.AddAsync(entity);

                    var baseUrl = _config["Environment:FE_URL"];
                    var riskDetailUrl = $"{baseUrl}/project/{project.ProjectKey}/risk/{riskKey}";

                    var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(task.Id);
                    var assignedUsers = new List<Account>();
                    foreach (var taskAssignment in taskAssignments)
                    {
                        var user = await _accountRepo.GetAccountById(taskAssignment.AccountId);
                        if (user != null)
                            assignedUsers.Add(user);
                    }
                    var emailTasks = assignedUsers
                        .Select(user => _emailService.SendOverdueTaskNotificationEmailAsync(
                            assigneeFullName: user.FullName ?? user.Username,
                            assigneeEmail: user.Email,
                            taskId: task.Id,
                            taskTitle: entity.Title,
                            projectKey: project.ProjectKey,
                            plannedEndDate: task.PlannedEndDate.Value,
                            taskDetailUrl: riskDetailUrl
                    ));

                    await Task.WhenAll(emailTasks);
                    //if (taskAssignments.Any())
                    //{
                    //    var emailTasks = taskAssignments
                    //        .Select(async taskAssignment =>
                    //        {
                    //            var assignedUser = await _accountRepo.GetAccountById(taskAssignment.AccountId);
                    //            if (assignedUser != null)
                    //            {
                    //                return _emailService.SendOverdueTaskNotificationEmailAsync(
                    //                    assigneeFullName: assignedUser.FullName ?? assignedUser.Username,
                    //                    assigneeEmail: assignedUser.Email,
                    //                    taskId: task.Id,
                    //                    taskTitle: entity.Title,
                    //                    projectKey: project.ProjectKey,
                    //                    plannedEndDate: task.PlannedEndDate.Value,
                    //                    taskDetailUrl: riskDetailUrl
                    //                );
                    //            }
                    //            return Task.CompletedTask;
                    //        })
                    //        .ToArray();

                    //    await Task.WhenAll(emailTasks);
                    //}
                }
            }
        }

        public async Task CheckAndNotifyOverdueRisksAsync()
        {
            var today = DateTime.UtcNow.Date;
            var risks = await _riskRepo.GetAllRisksAsync();

            var overdueRisks = risks
                .Where(r => !string.Equals(r.Status, "CLOSED", StringComparison.OrdinalIgnoreCase) &&
                            r.DueDate.HasValue &&
                            r.DueDate.Value < today)
                .ToList();

            foreach (var risk in overdueRisks)
            {
                Console.WriteLine($"Processing risk {risk.RiskKey}: Title={risk.Title}, DueDate={risk.DueDate}, Status={risk.Status}");
                if (risk.ResponsibleId.HasValue)
                {
                    var responsible = await _accountRepo.GetAccountById(risk.ResponsibleId.Value);
                    if (responsible != null)
                    {
                        var baseUrl = _config["Environment:FE_URL"];
                        var riskDetailUrl = $"{baseUrl}/project/{(await _projectRepo.GetByIdAsync(risk.ProjectId))?.ProjectKey}/risk/{risk.RiskKey}";

                        await _emailService.SendOverdueRiskNotificationEmailAsync(
                            assigneeFullName: responsible.FullName ?? responsible.Username,
                            assigneeEmail: responsible.Email,
                            riskKey: risk.RiskKey,
                            riskTitle: risk.Title,
                            projectKey: (await _projectRepo.GetByIdAsync(risk.ProjectId))?.ProjectKey,
                            dueDate: risk.DueDate.Value,
                            riskDetailUrl: riskDetailUrl
                        );
                    }
                }
            }
        }

        public async Task CheckAndNotifyOverdueRisksByProjectAsync(string projectKey)
        {
            var today = DateTime.UtcNow.Date;
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found with provided projectKey");

            var risks = await _riskRepo.GetByProjectIdAsync(project.Id);

            var overdueRisks = risks
                .Where(r => !string.Equals(r.Status, RiskStatusEnum.CLOSED.ToString(), StringComparison.OrdinalIgnoreCase) &&
                            r.DueDate.HasValue &&
                            r.DueDate.Value < today)
                .ToList();

            foreach (var risk in overdueRisks)
            {
                Console.WriteLine($"Processing risk {risk.RiskKey}: Title={risk.Title}, DueDate={risk.DueDate}, Status={risk.Status}");
                if (risk.ResponsibleId.HasValue)
                {
                    var responsible = await _accountRepo.GetAccountById(risk.ResponsibleId.Value);
                    if (responsible != null)
                    {
                        var baseUrl = _config["Environment:FE_URL"];
                        var riskDetailUrl = $"{baseUrl}/project/{projectKey}/risk/{risk.RiskKey}";

                        await _emailService.SendOverdueRiskNotificationEmailAsync(
                            assigneeFullName: responsible.FullName ?? responsible.Username,
                            assigneeEmail: responsible.Email,
                            riskKey: risk.RiskKey,
                            riskTitle: risk.Title,
                            projectKey: projectKey,
                            dueDate: risk.DueDate.Value,
                            riskDetailUrl: riskDetailUrl
                        );
                    }
                }
            }
        }

        private string GenerateUniqueRiskKey(string projectKey, int index)
        {
            var baseKey = $"{projectKey}-R{index:D3}";
            var existingRisk = _riskRepo.GetByKeyAsync(baseKey).Result; 
            if (existingRisk != null)
            {
                return GenerateUniqueRiskKey(projectKey, index + 1);
            }
            return baseKey;
        }

        public async Task<RiskStatisticsDTO> GetRiskStatisticsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null) throw new Exception("Project not found");

            var risks = await _riskRepo.GetByProjectIdAsync(project.Id);
            var accounts = await _accountRepo.GetAccounts();

            var stats = new RiskStatisticsDTO
            {
                TotalRisks = risks.Count,
                OverdueRisks = risks.Count(r => r.DueDate < DateTime.UtcNow && !r.Status.ToUpper().Contains(RiskStatusEnum.CLOSED.ToString())),
                RisksByStatus = risks.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count()),
                RisksByType = risks.GroupBy(r => r.Type).ToDictionary(g => g.Key, g => g.Count()),
                RisksBySeverity = risks.GroupBy(r => r.SeverityLevel).ToDictionary(g => g.Key, g => g.Count()),
                RisksByResponsible = risks
                    .GroupJoin(
                        accounts,
                        risk => risk.ResponsibleId,
                        account => account.Id,
                        (risk, accs) => new { Risk = risk, Account = accs.FirstOrDefault() }
                    )
                    .GroupBy(
                        x => x.Account?.FullName ?? x.Account?.Username ?? "Unassigned",
                        x => x.Risk,
                        (key, group) => new { Key = key, Count = group.Count() }
                    )
                    .ToDictionary(x => x.Key, x => x.Count)
            };

            return stats;
        }
    }

}
