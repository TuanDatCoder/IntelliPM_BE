using IntelliPM.Data.DTOs.SystemConfiguration.Request;
using IntelliPM.Data.DTOs.SystemConfiguration.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SystemConfigurationServices
{
    public interface ISystemConfigurationService
    {
        Task<List<SystemConfigurationResponseDTO>> GetAllSystemConfigurations();
        Task<SystemConfigurationResponseDTO> GetSystemConfigurationById(int id);
        Task<SystemConfigurationResponseDTO> GetSystemConfigurationByConfigKey(string configKey);
        Task<SystemConfigurationResponseDTO> CreateSystemConfiguration(SystemConfigurationRequestDTO request);
        Task<SystemConfigurationResponseDTO> UpdateSystemConfiguration(int id, SystemConfigurationRequestDTO request);
        Task DeleteSystemConfiguration(int id);
    }
}
