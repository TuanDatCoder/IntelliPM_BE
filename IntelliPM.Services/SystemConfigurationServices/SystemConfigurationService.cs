using AutoMapper;
using IntelliPM.Data.DTOs.SystemConfiguration.Request;
using IntelliPM.Data.DTOs.SystemConfiguration.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.SystemConfigurationRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SystemConfigurationServices
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly ISystemConfigurationRepository _repo;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(IMapper mapper, ISystemConfigurationRepository repo, ILogger<SystemConfigurationService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<SystemConfigurationResponseDTO>> GetAllSystemConfigurations()
        {
            var entities = await _repo.GetAllSystemConfigurations();
            return _mapper.Map<List<SystemConfigurationResponseDTO>>(entities);
        }

        public async Task<SystemConfigurationResponseDTO> GetSystemConfigurationById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"System configuration with ID {id} not found.");

            return _mapper.Map<SystemConfigurationResponseDTO>(entity);
        }

        public async Task<SystemConfigurationResponseDTO> GetSystemConfigurationByConfigKey(string configKey)
        {
            if (string.IsNullOrEmpty(configKey))
                throw new ArgumentNullException(nameof(configKey), "Config key cannot be null or empty.");

            var entity = await _repo.GetByConfigKeyAsync(configKey);
            if (entity == null)
                throw new KeyNotFoundException($"System configuration with ConfigKey '{configKey}' not found.");

            return _mapper.Map<SystemConfigurationResponseDTO>(entity);
        }

        public async Task<SystemConfigurationResponseDTO> CreateSystemConfiguration(SystemConfigurationRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.ConfigKey))
                throw new ArgumentException("ConfigKey is required.", nameof(request.ConfigKey));

            var entity = _mapper.Map<SystemConfiguration>(request);

            // Kiểm tra trùng lặp ConfigKey trước khi lưu (nếu repository không tự xử lý)
            var existingConfig = await _repo.GetByConfigKeyAsync(entity.ConfigKey);
            if (existingConfig != null)
                throw new InvalidOperationException($"A system configuration with ConfigKey '{entity.ConfigKey}' already exists.");

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi cụ thể từ database (ví dụ: vi phạm ràng buộc UNIQUE)
                throw new Exception($"Failed to create system configuration due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create system configuration: {ex.Message}", ex);
            }

            return _mapper.Map<SystemConfigurationResponseDTO>(entity);
        }

        public async Task<SystemConfigurationResponseDTO> UpdateSystemConfiguration(int id, SystemConfigurationRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"System configuration with ID {id} not found.");

            _mapper.Map(request, entity);
            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update system configuration: {ex.Message}", ex);
            }

            return _mapper.Map<SystemConfigurationResponseDTO>(entity);
        }

        public async Task DeleteSystemConfiguration(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"System configuration with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete system configuration: {ex.Message}", ex);
            }
        }
    }
}