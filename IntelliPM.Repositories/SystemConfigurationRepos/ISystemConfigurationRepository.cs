using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SystemConfigurationRepos
{
    public interface ISystemConfigurationRepository
    {
        Task<List<SystemConfiguration>> GetAllSystemConfigurations();
        Task<SystemConfiguration?> GetByIdAsync(int id);
        Task<SystemConfiguration?> GetByConfigKeyAsync(string configKey);
        Task Add(SystemConfiguration systemConfiguration);
        Task Update(SystemConfiguration systemConfiguration);
        Task Delete(SystemConfiguration systemConfiguration);
    }
}
