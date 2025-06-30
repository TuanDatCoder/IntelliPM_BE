using AutoMapper;
using IntelliPM.Data.DTOs.Label.Request;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.LabelRepos;
using IntelliPM.Repositories.ProjectRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.LabelServices
{
    public class LabelService : ILabelService
    {
        private readonly IMapper _mapper;
        private readonly ILabelRepository _repo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<LabelService> _logger;

        public LabelService(IMapper mapper, ILabelRepository repo, IProjectRepository projectRepo, ILogger<LabelService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _projectRepo = projectRepo;
        }

        public async Task<LabelResponseDTO> CreateLabel(LabelRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Label name is required.", nameof(request.Name));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            var entity = _mapper.Map<Label>(request);
            entity.Status = request.Status ?? "ACTIVE"; // Đặt giá trị mặc định nếu không có

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create label due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create label: {ex.Message}", ex);
            }
            return _mapper.Map<LabelResponseDTO>(entity);
        }

        public async Task DeleteLabel(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete label: {ex.Message}", ex);
            }
        }

        public async Task<List<LabelResponseDTO>> GetAllLabelAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) throw new ArgumentException("Invalid page or page size");
            var entities = (await _repo.GetAllLabelAsync())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<LabelResponseDTO>>(entities);
        }

        public async Task<LabelResponseDTO> GetLabelById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            return _mapper.Map<LabelResponseDTO>(entity);
        }

        public async Task<LabelResponseDTO> UpdateLabel(int id, LabelRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            _mapper.Map(request, entity);
            entity.Status = request.Status ?? entity.Status; // Giữ giá trị cũ nếu không có

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update label: {ex.Message}", ex);
            }

            return _mapper.Map<LabelResponseDTO>(entity);
        }
    }
}
