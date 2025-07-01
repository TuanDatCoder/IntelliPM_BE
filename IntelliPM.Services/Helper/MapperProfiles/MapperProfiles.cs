using AutoMapper;
using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.EpicComment.Request;
using IntelliPM.Data.DTOs.EpicComment.Response;
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
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
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
using IntelliPM.Data.DTOs.Task;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget));

            CreateMap<Project, ProjectResponseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProjectType, opt => opt.MapFrom(src => src.ProjectType));

            CreateMap<Project, ProjectDetailsDTO>()
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requirement))
                .ForMember(dest => dest.ProjectMembers, opt => opt.MapFrom(src => src.ProjectMember));

            // ProjectMetric
            CreateMap<ProjectMetric, ProjectMetricResponseDTO>();
            CreateMap<ProjectMetricRequestDTO, ProjectMetric>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
                

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
            CreateMap<Epic, EpicResponseDTO>();

            CreateMap<Epic, EpicDetailedResponseDTO>()
                .ForMember(dest => dest.ReporterFullname, opt => opt.Ignore())
                .ForMember(dest => dest.ReporterPicture, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByFullname, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByPicture, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.EpicComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));

            // Sprint
            CreateMap<SprintRequestDTO, Sprint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
            CreateMap<Sprint, SprintResponseDTO>();

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
            CreateMap<Tasks, TaskResponseDTO>();

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
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.TaskComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));

            CreateMap<TaskResponseDTO, Tasks>();


            // Requirement
            CreateMap<RequirementRequestDTO, Requirement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Requirement, RequirementResponseDTO>();
            CreateMap<RequirementResponseDTO, RequirementRequestDTO>();

            //Risk
            CreateMap<Risk, RiskResponseDTO>()
                .ForMember(dest => dest.ResponsibleName, opt => opt.MapFrom(src => src.Responsible.FullName))
                .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : null));

            CreateMap<RiskRequestDTO, Risk>();
            CreateMap<Risk, RiskRequestDTO>()
                .ForMember(dest => dest.MitigationPlan, opt => opt.MapFrom(src => src.RiskSolution.FirstOrDefault().MitigationPlan))
                .ForMember(dest => dest.ContingencyPlan, opt => opt.MapFrom(src => src.RiskSolution.FirstOrDefault().ContingencyPlan));

            //RiskSolution
            CreateMap<RiskSolution, RiskSolutionResponseDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToLocalTime()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToLocalTime()));

            CreateMap<RiskSolutionRequestDTO, RiskSolution>();
            CreateMap<RiskWithSolutionDTO, RiskRequestDTO>();

            CreateMap<RiskWithSolutionDTO, Risk>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<Risk, RiskRequestDTO>();



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
    .ForMember(dest => dest.AssignedByName,
        opt => opt.MapFrom(src => src.AssignedByNavigation != null
            ? src.AssignedByNavigation.FullName
            : null));


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
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.SubtaskComment))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.WorkItemLabel.Select(w => w.Label)));

            // ProjectMember
            CreateMap<ProjectMemberRequestDTO, ProjectMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ProjectMember, ProjectMemberResponseDTO>();
            CreateMap<ProjectMember, ProjectMemberWithPositionsResponseDTO>()
              .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.JoinedAt))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
              .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null))
              .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
              .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.Account != null ? src.Account.Picture : null))
              .ForMember(dest => dest.ProjectPositions, opt => opt.MapFrom(src => src.ProjectPosition)); ;

            // ProjectPosition
            CreateMap<ProjectPosition, ProjectPositionResponseDTO>();



            // TaskComment
            CreateMap<TaskCommentRequestDTO, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<TaskComment, TaskCommentResponseDTO>();

            // SubtaskComment
            CreateMap<SubtaskCommentRequestDTO, SubtaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<SubtaskComment, SubtaskCommentResponseDTO>();

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

            // TaskAssignment
            CreateMap<TaskAssignmentRequestDTO, TaskAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.Task, opt => opt.Ignore());
            CreateMap<TaskAssignment, TaskAssignmentResponseDTO>();

            //MeetingSummary
            CreateMap<MeetingSummary, MeetingSummaryResponseDTO>();
            CreateMap<MeetingSummaryRequestDTO, MeetingSummary>();


            //MeetngReschedleRequestRepos
            CreateMap<MeetingRescheduleRequest, MeetingRescheduleRequestResponseDTO>();
            CreateMap<MeetingRescheduleRequestDTO, MeetingRescheduleRequest>();

            // EpicComment Mapping
            CreateMap<EpicCommentRequestDTO, EpicComment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<EpicComment, EpicCommentResponseDTO>();

            // Label Mapping
            CreateMap<LabelRequestDTO, Label>()
                .ReverseMap();
            CreateMap<Label, LabelResponseDTO>();

            // WorkItemLabel Mapping
            CreateMap<WorkItemLabelRequestDTO, WorkItemLabel>()
                .ReverseMap();
            CreateMap<WorkItemLabel, WorkItemLabelResponseDTO>();


        }
    }
}