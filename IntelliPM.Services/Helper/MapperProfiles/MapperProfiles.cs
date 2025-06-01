using AutoMapper;
using IntelliPM.Data.DTOs.Account.Request;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.SystemConfiguration.Request;
using IntelliPM.Data.DTOs.SystemConfiguration.Response;
using IntelliPM.Data.Entities;

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
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth)); // Ánh xạ trực tiếp DateOnly sang DateOnly?

            CreateMap<Account, AccountResponseDTO>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth)); // DateOnly? sang DateOnly?

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
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Project, ProjectResponseDTO>();
        }
    }
}