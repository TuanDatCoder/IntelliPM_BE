using AutoMapper;
using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.ActivityLog.Request;
using IntelliPM.Data.DTOs.ActivityLog.Response;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.EpicComment.Request;
using IntelliPM.Data.DTOs.EpicComment.Response;
using IntelliPM.Data.DTOs.EpicFile.Request;
using IntelliPM.Data.DTOs.EpicFile.Response;
using IntelliPM.Data.DTOs.Label.Request;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Data.DTOs.MeetingLog.Response;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request;
using IntelliPM.Data.DTOs.MeetingRescheduleRequest.Response;
using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using IntelliPM.Data.DTOs.MeetingTranscript.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Data.DTOs.MilestoneFeedback.Response;
using IntelliPM.Data.DTOs.Notification.Request;
using IntelliPM.Data.DTOs.Notification.Response;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.DTOs.RecipientNotification.Request;
using IntelliPM.Data.DTOs.RecipientNotification.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskComment.Request;
using IntelliPM.Data.DTOs.RiskComment.Response;
using IntelliPM.Data.DTOs.RiskFile.Request;
using IntelliPM.Data.DTOs.RiskFile.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.SubtaskComment.Request;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.SubtaskFile.Request;
using IntelliPM.Data.DTOs.SubtaskFile.Response;
using IntelliPM.Data.DTOs.SystemConfiguration.Request;
using IntelliPM.Data.DTOs.SystemConfiguration.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.DTOs.TaskDependency.Request;
using IntelliPM.Data.DTOs.TaskDependency.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using IntelliPM.Data.DTOs.WorkLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Services.AiServices.TaskPlanningServices;

namespace IntelliPM.Services.Helper.MapperProfiles
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            // Account
            CreateMap<AccountRequestDTO, Account>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));

            CreateMap<Account, AccountResponseDTO>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));

            CreateMap<Account, AccountInformationResponseDTO>();

            // DynamicCategory
            CreateMap<DynamicCategoryRequestDTO, DynamicCategory>();
            CreateMap<DynamicCategory, DynamicCategoryResponseDTO>();

            // SystemConfiguration
            CreateMap<SystemConfigurationRequestDTO, SystemConfiguration>();
            CreateMap<SystemConfiguration, SystemConfigurationResponseDTO>();

            // Project
            CreateMap<ProjectRequestDTO, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget));

            CreateMap<Project, ProjectResponseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType));

            CreateMap<Project, ProjectDetailsDTO>()
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requirement))
                .ForMember(dest => dest.ProjectMembers, opt => opt.MapFrom(src => src.ProjectMember));

            CreateMap<Project, ProjectViewDTO>();

            // ProjectMetric
            CreateMap<ProjectMetric, ProjectMetricResponseDTO>();
            CreateMap<ProjectMetricRequestDTO, ProjectMetric>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<ProjectMetric, NewProjectMetricResponseDTO>();

            // ProjectRecommendation
            CreateMap<ProjectRecommendation, ProjectRecommendationResponseDTO>()
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task.Title));

            // Epic
            CreateMap<EpicRequestDTO, Epic>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            CreateMap<Epic, EpicResponseDTO>()
                .ForMember(dest => dest.AssignedByFullname, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.FullName : null))
                .ForMember(dest => dest.AssignedByPicture, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.Picture : null))
                .ForMember(dest => dest.ReporterFullname, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null))
                .ForMember(dest => dest.ReporterPicture, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Picture : null))
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.SprintGoal, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Goal : null));

            CreateMap<Epic, EpicWithStatsResponseDTO>()
            .ForMember(dest => dest.AssignedByFullname, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.FullName : null))
            .ForMember(dest => dest.AssignedByPicture, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.Picture : null))
            .ForMember(dest => dest.ReporterFullname, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null))
            .ForMember(dest => dest.ReporterPicture, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Picture : null))
            .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
            .ForMember(dest => dest.SprintGoal, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Goal : null));

            CreateMap<Epic, EpicDetailedResponseDTO>()
                .ForMember(dest => dest.ReporterFullname, opt => opt.Ignore())
                .ForMember(dest => dest.ReporterPicture, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByFullname, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByPicture, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
                 .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.EpicComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));

            CreateMap<EpicWithTaskRequestDTO, Epic>()
            .ForMember(dest => dest.Tasks, opt => opt.Ignore());
            CreateMap<EpicTaskAssignedMembersRequestDTO, Tasks>();
            CreateMap<TaskAssignedMembersRequestDTO, TaskAssignment>();



            // Sprint
            CreateMap<SprintRequestDTO, Sprint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
            CreateMap<Sprint, SprintResponseDTO>();
            CreateMap<Sprint, SprintWithTaskListResponseDTO>();

            // Milestone
            CreateMap<MilestoneRequestDTO, Milestone>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
            CreateMap<Milestone, MilestoneResponseDTO>();

            // Task
            CreateMap<TaskRequestDTO, Tasks>();
            CreateMap<TaskUpdateRequestDTO, Tasks>();

            CreateMap<Tasks, TaskResponseDTO>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null))
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.EpicName, opt => opt.MapFrom(src => src.Epic != null ? src.Epic.Name : null))
                .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.TaskDependencyTask));

            CreateMap<Tasks, TaskUpdateResponseDTO>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
                  .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.EpicName, opt => opt.MapFrom(src => src.Epic != null ? src.Epic.Name : null))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null));

            CreateMap<TaskWithMembersDTO, TaskRequestDTO>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.PlannedStartDate, opt => opt.MapFrom(src => src.StartDate))
               .ForMember(dest => dest.PlannedEndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.ReporterId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.EpicId, opt => opt.Ignore())
                .ForMember(dest => dest.SprintId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<Tasks, TaskDetailedResponseDTO>()
                .ForMember(dest => dest.ReporterFullname, opt => opt.Ignore())
                .ForMember(dest => dest.ReporterPicture, opt => opt.Ignore())
                .ForMember(dest => dest.TaskAssignments, opt => opt.MapFrom(src => src.TaskAssignment))
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));

            CreateMap<TaskResponseDTO, Tasks>();

            CreateMap<TaskUpdateResponseDTO, Tasks>();

            CreateMap<Tasks, TaskBacklogResponseDTO>()
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null))
                .ForMember(dest => dest.ReporterPicture, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Picture : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
                .ForMember(dest => dest.EpicName, opt => opt.MapFrom(src => src.Epic != null ? src.Epic.Name : null))
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null));

            CreateMap<Tasks, TaskSubtaskDependencyResponseDTO>();

            // Task Dependency
            CreateMap<TaskDependencyRequestDTO, TaskDependency>();
            CreateMap<TaskDependency, TaskDependencyResponseDTO>();

            // Requirement
            CreateMap<RequirementRequestDTO, Requirement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Requirement, RequirementResponseDTO>();

            CreateMap<TaskAssignment, TaskAssignmentResponseDTO>()
                .ForMember(dest => dest.AccountFullname, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            CreateMap<RequirementNoProjectRequestDTO, Requirement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<RequirementResponseDTO, RequirementNoProjectRequestDTO>();
            CreateMap<RequirementResponseDTO, RequirementRequestDTO>();
            CreateMap<RequirementBulkRequestDTO, RequirementNoProjectRequestDTO>();

            // Risk
            CreateMap<Risk, RiskResponseDTO>()
                .ForMember(dest => dest.ResponsibleFullName, opt => opt.MapFrom(src => src.Responsible.FullName))
                .ForMember(dest => dest.ResponsibleUserName, opt => opt.MapFrom(src => src.Responsible.Username))
                .ForMember(dest => dest.ResponsiblePicture, opt => opt.MapFrom(src => src.Responsible.Picture))
                .ForMember(dest => dest.CreatorFullName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.CreatorUserName, opt => opt.MapFrom(src => src.CreatedByNavigation.Username))
                .ForMember(dest => dest.CreatorPicture, opt => opt.MapFrom(src => src.CreatedByNavigation.Picture))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null));
            CreateMap<RiskRequestDTO, Risk>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<RiskCreateRequestDTO, Risk>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // RiskSolution
            CreateMap<RiskSolution, RiskSolutionResponseDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToLocalTime()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToLocalTime()));
            CreateMap<RiskSolutionRequestDTO, RiskSolution>();

            // RiskFile
            CreateMap<RiskFileRequestDTO, RiskFile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());
            CreateMap<RiskFile, RiskFileResponseDTO>();

            // RiskComment
            CreateMap<RiskCommentRequestDTO, RiskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<RiskComment, RiskCommentResponseDTO>()
                 .ForMember(dest => dest.AccountFullname, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                 .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                 .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            // Meeting
            CreateMap<MeetingRequestDTO, Meeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "ACTIVE"));
            CreateMap<Meeting, MeetingResponseDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // MeetingParticipant
            CreateMap<MeetingParticipantRequestDTO, MeetingParticipant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<MeetingParticipant, MeetingParticipantResponseDTO>();

            // Subtask
            CreateMap<SubtaskRequestDTO, Subtask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Subtask, SubtaskResponseDTO>()
                .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.FullName : null))
                .ForMember(dest => dest.AssignedByPicture, opt => opt.MapFrom(src => src.AssignedByNavigation != null ? src.AssignedByNavigation.Picture : null))
                .ForMember(dest => dest.ReporterPicture, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Picture : null))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : null));
            CreateMap<SubtaskRequest1DTO, Subtask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<SubtaskRequest2DTO, Subtask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Subtask, SubtaskDetailedResponseDTO>()
                .ForMember(dest => dest.ReporterFullname, opt => opt.Ignore())
                .ForMember(dest => dest.ReporterPicture, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByFullname, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByPicture, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
                                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint != null ? src.Sprint.Name : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.SubtaskComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));
            CreateMap<Subtask, SubtaskFullResponseDTO>();

            // ProjectMember
            CreateMap<ProjectMemberRequestDTO, ProjectMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProjectMember, ProjectMemberResponseDTO>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null));
            CreateMap<ProjectMember, ProjectMemberWithPositionsResponseDTO>()
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null))
                .ForMember(dest => dest.ProjectPositions, opt => opt.MapFrom(src => src.ProjectPosition));

            CreateMap<ProjectMemberNoProjectIdRequestDTO, ProjectMember>();

            // ProjectPosition
            CreateMap<ProjectPosition, ProjectPositionResponseDTO>();
            CreateMap<ProjectPositionNoMemberIdRequestDTO, ProjectPosition>();

            // TaskComment
            CreateMap<TaskCommentRequestDTO, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<TaskComment, TaskCommentResponseDTO>()
                 .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                 .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            // ActivityLog
            CreateMap<ActivityLogRequestDTO, ActivityLog>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ActivityLog, ActivityLogResponseDTO>()
                 .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.FullName : null));

            // RecipientNotification
            CreateMap<RecipientNotificationRequestDTO, RecipientNotification>()
               .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<RecipientNotification, RecipientNotificationResponseDTO>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                .ForMember(dest => dest.NotificationMessage, opt => opt.MapFrom(src => src.Notification != null ? src.Notification.Message : null));

            // Notification
            CreateMap<NotificationRequestDTO, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Notification, NotificationResponseDTO>()
                 .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.FullName : null));

            // SubtaskComment
            CreateMap<SubtaskCommentRequestDTO, SubtaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<SubtaskComment, SubtaskCommentResponseDTO>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            CreateMap<MeetingLogRequestDTO, MeetingLog>();
            CreateMap<MeetingLog, MeetingLogResponseDTO>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.FullName));

            // Mapping cho MeetingTranscript
            CreateMap<MeetingTranscriptRequestDTO, MeetingTranscript>();
            CreateMap<MeetingTranscript, MeetingTranscriptResponseDTO>();

            // MilestoneFeedback
            CreateMap<MilestoneFeedbackRequestDTO, MilestoneFeedback>();
            CreateMap<MilestoneFeedback, MilestoneFeedbackResponseDTO>();

            // TaskFile
            CreateMap<TaskFileRequestDTO, TaskFile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<TaskFile, TaskFileResponseDTO>();

            // SubtaskFile
            CreateMap<SubtaskFileRequestDTO, SubtaskFile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<SubtaskFile, SubtaskFileResponseDTO>();

            // EpicFile
            CreateMap<EpicFileRequestDTO, EpicFile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<EpicFile, EpicFileResponseDTO>();

            // TaskAssignment
            CreateMap<TaskAssignmentRequestDTO, TaskAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore());
            CreateMap<TaskAssignment, TaskAssignmentResponseDTO>()
                .ForMember(dest => dest.AccountFullname, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
                .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            CreateMap<TaskAssignmentQuickRequestDTO, TaskAssignment>();
            CreateMap<TaskAssignment, TaskAssignmentRequestDTO>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.PlannedHours, opt => opt.MapFrom(src => src.PlannedHours));

            // MeetingSummary
            CreateMap<MeetingSummary, MeetingSummaryResponseDTO>();
            CreateMap<MeetingSummaryRequestDTO, MeetingSummary>();

            // MeetingRescheduleRequest
            CreateMap<MeetingRescheduleRequest, MeetingRescheduleRequestResponseDTO>();
            CreateMap<MeetingRescheduleRequestDTO, MeetingRescheduleRequest>();

            // EpicComment Mapping
            CreateMap<EpicCommentRequestDTO, EpicComment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<EpicComment, EpicCommentResponseDTO>()
               .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
               .ForMember(dest => dest.AccountPicture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null));

            // Label Mapping
            CreateMap<LabelRequestDTO, Label>()
                .ReverseMap();
            CreateMap<Label, LabelResponseDTO>();

            // WorkItemLabel Mapping
            CreateMap<WorkItemLabelRequestDTO, WorkItemLabel>()
                .ReverseMap();
            CreateMap<WorkItemLabel, WorkItemLabelResponseDTO>()
                .ForMember(dest => dest.LabelName, opt => opt.MapFrom(src => src.Label != null ? src.Label.Name : null));

            // WorkLog
            CreateMap<WorkLog, WorkLogResponseDTO>();
        }
    }
}