using AutoMapper;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.SprintRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SprintServices
{
    public class SprintService : ISprintService
    {
        private readonly IMapper _mapper;
        private readonly ISprintRepository _repo;
        private readonly ILogger<SprintService> _logger;

        public SprintService(IMapper mapper, ISprintRepository repo, ILogger<SprintService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<SprintResponseDTO>> GetAllSprints()
        {
            var entities = await _repo.GetAllSprints();
            return _mapper.Map<List<SprintResponseDTO>>(entities);
        }

        public async Task<SprintResponseDTO> GetSprintById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            return _mapper.Map<SprintResponseDTO>(entity);
        }

        public async Task<List<SprintResponseDTO>> GetSprintByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var entities = await _repo.GetByNameAsync(name);
            if (!entities.Any())
                throw new KeyNotFoundException($"No sprints found with name containing '{name}'.");

            return _mapper.Map<List<SprintResponseDTO>>(entities);
        }

        public async Task<SprintResponseDTO> CreateSprint(SprintRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Sprint name is required.", nameof(request.Name));

            var entity = _mapper.Map<Sprint>(request);
            //entity.CreatedAt = DateTime.UtcNow; // Gán ngày tạo hiện tại
            //entity.UpdatedAt = DateTime.UtcNow; // Gán ngày cập nhật hiện tại

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create sprint due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create sprint: {ex.Message}", ex);
            }

            return _mapper.Map<SprintResponseDTO>(entity);
        }

        public async Task<SprintResponseDTO> UpdateSprint(int id, SprintRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow; // Cập nhật ngày hiện tại khi sửa

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update sprint: {ex.Message}", ex);
            }

            return _mapper.Map<SprintResponseDTO>(entity);
        }

        public async Task DeleteSprint(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete sprint: {ex.Message}", ex);
            }
        }

        public async Task<SprintResponseDTO> ChangeSprintStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow; // Cập nhật ngày khi thay đổi trạng thái

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change sprint status: {ex.Message}", ex);
            }

            return _mapper.Map<SprintResponseDTO>(entity);
        }
    }
}
