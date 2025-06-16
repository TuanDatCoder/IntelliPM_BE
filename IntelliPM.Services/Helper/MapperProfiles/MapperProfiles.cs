

﻿using AutoMapper;
using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.SystemConfiguration.Request;
using IntelliPM.Data.DTOs.SystemConfiguration.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;


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
            CreateMap<TaskRequestDTO, Tasks>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.PlannedStartDate, opt => opt.MapFrom(src => src.PlannedStartDate))
                .ForMember(dest => dest.PlannedEndDate, opt => opt.MapFrom(src => src.PlannedEndDate))
                .ForMember(dest => dest.ActualStartDate, opt => opt.MapFrom(src => src.ActualStartDate))
                .ForMember(dest => dest.ActualEndDate, opt => opt.MapFrom(src => src.ActualEndDate));
            CreateMap<Tasks, TaskResponseDTO>();

            // ProjectMember
            CreateMap<ProjectMemberRequestDTO, ProjectMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.JoinedAt, opt => opt.Ignore());
            CreateMap<ProjectMember, ProjectMemberResponseDTO>();

            // ProjectPosition
            CreateMap<ProjectPositionRequestDTO, ProjectPosition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedAt, opt => opt.Ignore());
            CreateMap<ProjectPosition, ProjectPositionResponseDTO>();

            // Requirement
            CreateMap<RequirementRequestDTO, Requirement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); 
            CreateMap<Requirement, RequirementResponseDTO>();


            // Ánh xạ MeetingRequestDTO -> Meeting
            CreateMap<MeetingRequestDTO, Meeting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())  // Giữ nguyên ID của Meeting khi tạo mới
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))  // Tự động set CreatedAt khi tạo mới
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "ACTIVE"));  // Mặc định set Status là "ACTIVE"

            // Ánh xạ Meeting -> MeetingResponseDTO
            CreateMap<Meeting, MeetingResponseDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));


            CreateMap<MeetingParticipantRequestDTO, MeetingParticipant>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<MeetingParticipant, MeetingParticipantResponseDTO>();
            // TaskCheckList
            CreateMap<TaskCheckListRequestDTO, TaskCheckList>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<TaskCheckList, TaskCheckListResponseDTO>();
            // TaskCheckList
            CreateMap<TaskCheckListRequestDTO, TaskCheckList>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<TaskCheckList, TaskCheckListResponseDTO>();

            // TaskComment
            CreateMap<TaskCommentRequestDTO, TaskComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<TaskComment, TaskCommentResponseDTO>();

        }
    }

}