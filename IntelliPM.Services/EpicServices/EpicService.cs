using AutoMapper;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicServices
{
    public class EpicService : IEpicService
    {
        private readonly IMapper _mapper;
        private readonly IEpicRepository _repo;
        private readonly ILogger<EpicService> _logger;

        public EpicService(IMapper mapper, IEpicRepository repo, ILogger<EpicService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<EpicResponseDTO>> GetAllEpics()
        {
            var entities = await _repo.GetAllEpics();
            return _mapper.Map<List<EpicResponseDTO>>(entities);
        }

        public async Task<EpicResponseDTO> GetEpicById(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task<List<EpicResponseDTO>> GetEpicByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var entities = await _repo.GetByNameAsync(name);
            if (!entities.Any())
                throw new KeyNotFoundException($"No epics found with name containing '{name}'.");

            return _mapper.Map<List<EpicResponseDTO>>(entities);
        }

        public async Task<EpicResponseDTO> CreateEpic(EpicRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Epic name is required.", nameof(request.Name));

            var entity = _mapper.Map<Epic>(request);
            //entity.CreatedAt = DateTime.UtcNow; // Gán ngày tạo hiện tại
            //entity.UpdatedAt = DateTime.UtcNow; // Gán ngày cập nhật hiện tại

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create epic due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create epic: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task<EpicResponseDTO> UpdateEpic(string id, EpicRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow; // Cập nhật ngày hiện tại khi sửa

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update epic: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task DeleteEpic(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete epic: {ex.Message}", ex);
            }
        }

        public async Task<EpicResponseDTO> ChangeEpicStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow; // Cập nhật ngày khi thay đổi trạng thái

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change epic status: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }
    }

}
