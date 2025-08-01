using AutoMapper;
using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
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
        private readonly ISprintRepository _sprintRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<MilestoneService> _logger;

        public MilestoneService(IMapper mapper, IMilestoneRepository repo, IProjectRepository projectRepo, ISprintRepository sprintRepo,ILogger<MilestoneService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _sprintRepo = sprintRepo;
            _projectRepo = projectRepo;
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

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            string milestoneKey = await GenerateMilestoneKeyAsync(request.ProjectId, project.ProjectKey);

            var entity = _mapper.Map<Milestone>(request);
            entity.Key = milestoneKey; 

            try
            {
                _logger.LogInformation("Creating milestone with key: {Key} for project: {ProjectId}", milestoneKey, request.ProjectId);
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating milestone with key: {Key}", milestoneKey);
                throw new Exception($"Failed to create milestone due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating milestone with key: {Key}", milestoneKey);
                throw new Exception($"Failed to create milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        private async Task<string> GenerateMilestoneKeyAsync(int projectId, string projectKey)
        {
            var entities = await _repo.GetMilestonesByProjectIdAsync(projectId);
            int nextNumber = 1;

            if (entities.Any())
            {
                var latestKey = entities
                    .Select(m => m.Key?.Trim() ?? "")
                    .Where(k => k.StartsWith($"{projectKey}-M"))
                    .OrderByDescending(k => k)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(latestKey))
                {
                    var parts = latestKey.Split('-');
                    string lastPart = parts.LastOrDefault() ?? "";
                    if (lastPart.StartsWith("M") && int.TryParse(lastPart.Substring(1), out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }
            }

            return $"{projectKey}-M{nextNumber}";
        }







        public async Task<MilestoneResponseDTO> UpdateMilestone(int id, MilestoneRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

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
            entity.UpdatedAt = DateTime.UtcNow;


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



        public async Task<MilestoneResponseDTO> ChangeMilestoneSprint(string key, int sprintId)
        {
            var entity = await _repo.GetByKeyAsync(key);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with Key {key} not found.");

            if (sprintId == 0)
            {
                entity.SprintId = null;
            }
            else
            {
                var sprint = await _sprintRepo.GetByIdAsync(sprintId);
                if (sprint == null)
                    throw new KeyNotFoundException($"Sprint with ID {sprintId} not found.");
                entity.SprintId = sprintId;
            }


            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change milestone sprint: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }



        public async Task<List<MilestoneResponseDTO>> GetMilestonesByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _repo.GetMilestonesByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No milestones found for Project ID {projectId}.");

            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }

    }
}
