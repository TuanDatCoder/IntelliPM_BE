using AutoMapper;
using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MilestoneRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.MilestoneServices
{
    public class MilestoneService : IMilestoneService
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneRepository _repo;
        private readonly ILogger<MilestoneService> _logger;

        public MilestoneService(IMapper mapper, IMilestoneRepository repo, ILogger<MilestoneService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<MilestoneResponseDTO>> GetAllMilestones()
        {
            var entities = await _repo.GetAllMilestones();
            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }

        public async Task<MilestoneResponseDTO> GetMilestoneById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        public async Task<List<MilestoneResponseDTO>> GetMilestoneByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var entities = await _repo.GetByNameAsync(name);
            if (!entities.Any())
                throw new KeyNotFoundException($"No milestones found with name containing '{name}'.");

            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }

        public async Task<MilestoneResponseDTO> CreateMilestone(MilestoneRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Milestone name is required.", nameof(request.Name));

            var entity = _mapper.Map<Milestone>(request);
            // Không gán CreatedAt và UpdatedAt vì DB tự động gán

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create milestone due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        public async Task<MilestoneResponseDTO> UpdateMilestone(int id, MilestoneRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            _mapper.Map(request, entity);
            // Không gán UpdatedAt vì DB tự động cập nhật

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        public async Task DeleteMilestone(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete milestone: {ex.Message}", ex);
            }
        }

        public async Task<MilestoneResponseDTO> ChangeMilestoneStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            entity.Status = status;
            // Không gán UpdatedAt vì DB tự động cập nhật

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change milestone status: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }
    }
}
