using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SystemConfigurationRepos
{
    public class SystemConfigurationRepository : ISystemConfigurationRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public SystemConfigurationRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<SystemConfiguration>> GetAllSystemConfigurations()
        {
            return await _context.SystemConfiguration
                .OrderBy(sc => sc.Id)
                .ToListAsync();
        }

        public async Task<SystemConfiguration?> GetByIdAsync(int id)
        {
            return await _context.SystemConfiguration
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<SystemConfiguration?> GetByConfigKeyAsync(string configKey)
        {
            return await _context.SystemConfiguration
                .FirstOrDefaultAsync(sc => sc.ConfigKey == configKey);
        }

        public async Task Add(SystemConfiguration systemConfiguration)
        {
            await _context.SystemConfiguration.AddAsync(systemConfiguration);
            await _context.SaveChangesAsync();
        }

        public async Task Update(SystemConfiguration systemConfiguration)
        {
            _context.SystemConfiguration.Update(systemConfiguration);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(SystemConfiguration systemConfiguration)
        {
            _context.SystemConfiguration.Remove(systemConfiguration);
            await _context.SaveChangesAsync();
        }
    }
}