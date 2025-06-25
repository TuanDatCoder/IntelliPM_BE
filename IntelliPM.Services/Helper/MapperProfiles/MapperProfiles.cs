using AutoMapper;
using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Data.DTOs.MeetingLog.Response;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
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
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Subtask.Request;
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
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Entities;
using IntelliPM.Services.AiServices.TaskPlanningServices; // Thêm namespace cho TaskWithMembers
using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;

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
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Epic
            CreateMap<EpicRequestDTO, Epic>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));
            CreateMap<Epic, EpicResponseDTO>();

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

            CreateMap<TaskResponseDTO, Tasks>();


            // Requirement
            CreateMap<RequirementRequestDTO, Requirement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Requirement, RequirementResponseDTO>();
            CreateMap<RequirementResponseDTO, RequirementRequestDTO>();

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

            // TaskSubtask
            CreateMap<SubtaskRequestDTO, Subtask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Subtask, SubtaskResponseDTO>();

            CreateMap<SubtaskRequest1DTO, Subtask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Subtask, SubtaskResponseDTO>();

            // ProjectMember
            CreateMap<ProjectMemberRequestDTO, Data.Entities.ProjectMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Data.Entities.ProjectMember, ProjectMemberResponseDTO>();

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

        }
    }
}