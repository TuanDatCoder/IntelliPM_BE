using AutoMapper;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.RequirementRepos;
using IntelliPM.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RequirementServices
{
    public class RequirementService : IRequirementService
    {
        private readonly IMapper _mapper;
        private readonly IRequirementRepository _repo;
        private readonly ILogger<RequirementService> _logger;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly IProjectRepository _projectRepo;

        public RequirementService(IMapper mapper, IRequirementRepository repo, ILogger<RequirementService> logger, IDynamicCategoryRepository dynamicCategoryRepo, IProjectRepository projectRepo)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _projectRepo=projectRepo;

        }

        public async Task<List<RequirementResponseDTO>> GetAllRequirements(int projectId)
        {
            var entities = await _repo.GetAllRequirements(projectId);
            return _mapper.Map<List<RequirementResponseDTO>>(entities);
        }

        public async Task<RequirementResponseDTO> GetRequirementById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Requirement with ID {id} not found.");

            return _mapper.Map<RequirementResponseDTO>(entity);
        }

        public async Task<List<RequirementResponseDTO>> GetRequirementByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entities = await _repo.GetByTitleAsync(title);
            if (!entities.Any())
                throw new KeyNotFoundException($"No requirements found with title containing '{title}'.");

            return _mapper.Map<List<RequirementResponseDTO>>(entities);
        }

        public async Task<RequirementResponseDTO> CreateRequirement(int projectId, RequirementNoProjectRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            var existingProject = await _projectRepo.GetByIdAsync(projectId);
            if (existingProject == null)
            {
                // Ném ra một exception cụ thể hơn
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Requirement title is required.", nameof(request.Title));

            var entity = _mapper.Map<Requirement>(request);
            entity.ProjectId = projectId; 

            try
            {
                await _repo.Add(entity);
                entity.CreatedAt = DateTime.UtcNow; 
                entity.UpdatedAt = DateTime.UtcNow; 
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create requirement due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create requirement: {ex.Message}", ex);
            }

            return _mapper.Map<RequirementResponseDTO>(entity);
        }

        public async Task<RequirementResponseDTO> UpdateRequirement(int id, int projectId, RequirementNoProjectRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Requirement title is required.", nameof(request.Title));

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Requirement with ID {id} not found.");

            if (entity.ProjectId != projectId)
                throw new ArgumentException($"Requirement with ID {id} does not belong to project ID {projectId}.");

            _mapper.Map(request, entity);

            try
            {
                entity.ProjectId = projectId; 
                entity.UpdatedAt = DateTime.UtcNow;
                await _repo.Update(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to update requirement: An error occurred while saving the entity changes. Inner exception: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update requirement: {ex.Message}", ex);
            }

            return _mapper.Map<RequirementResponseDTO>(entity);
        }

        public async Task DeleteRequirement(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Requirement with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete requirement: {ex.Message}", ex);
            }
        }

        public async Task<RequirementResponseDTO> ChangeRequirementPriority(int id, string priority)
        {
            // Kiểm tra và ánh xạ priority
            string validatedPriority = await DynamicCategoryUtility.ValidateAndMapAsync(
                _dynamicCategoryRepo,
                "requirement_priority",
                priority,
                true);

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Requirement with ID {id} not found.");

            entity.Priority = validatedPriority;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change requirement priority for ID {Id}", id);
                throw new Exception($"Failed to change requirement priority: {ex.Message}", ex);
            }

            return _mapper.Map<RequirementResponseDTO>(entity);
        }

        public async Task<List<RequirementResponseDTO>> CreateListRequirement(int projectId, List<RequirementBulkRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("List of requirements cannot be null or empty.");

            var responses = new List<RequirementResponseDTO>();
            foreach (var request in requests)
            {
                var mappedRequest = _mapper.Map<RequirementNoProjectRequestDTO>(request);
                var response = await CreateRequirement(projectId,mappedRequest);
                responses.Add(response);
            }
            return responses;
        }


    }
}
