using AutoMapper;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectServices
{
    public class ProjectService : IProjectService
    {
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IMapper mapper, IProjectRepository projectRepo, ILogger<ProjectService> logger)
        {
            _mapper = mapper;
            _projectRepo = projectRepo;
            _logger = logger;
        }

        public async Task<List<ProjectResponseDTO>> GetAllProjects()
        {
            var entities = await _projectRepo.GetAllProjects();
            return _mapper.Map<List<ProjectResponseDTO>>(entities);
        }

        public async Task<ProjectResponseDTO> GetProjectById(int id)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task<List<ProjectResponseDTO>> SearchProjects(string searchTerm, string? projectType, string? status)
        {
            if (string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(projectType) && string.IsNullOrEmpty(status))
                throw new ArgumentException("At least one search criterion must be provided.");

            var entities = await _projectRepo.SearchProjects(searchTerm, projectType, status);
            return _mapper.Map<List<ProjectResponseDTO>>(entities);
        }

        public async Task<ProjectResponseDTO> CreateProject(ProjectRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Project name is required.", nameof(request.Name));

            var entity = _mapper.Map<Project>(request);

            try
            {
                await _projectRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create project due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create project: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task<ProjectResponseDTO> UpdateProject(int id, ProjectRequestDTO request)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _projectRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update project: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task DeleteProject(int id)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            try
            {
                await _projectRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project: {ex.Message}", ex);
            }
        }

        public async Task<ProjectDetailsDTO> GetProjectDetails(int id)
        {
            var project = await _projectRepo.GetProjectWithMembersAndRequirements(id);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            var details = new ProjectDetailsDTO
            {
                Id = project.Id,
                ProjectKey = project.ProjectKey,
                Name = project.Name,
                Description = project.Description,
                Budget = project.Budget,
                ProjectType = project.ProjectType,
                CreatedBy = project.CreatedBy,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Status = project.Status,

                Requirements = project.Requirement?.Select(r => new RequirementResponseDTO
                {
                    Id = r.Id,
                    ProjectId = r.ProjectId,
                    Title = r.Title,
                    Type = r.Type,
                    Description = r.Description,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList(),

                ProjectMembers = project.ProjectMember?.Select(pm => new ProjectMemberWithPositionsResponseDTO
                {
                    Id = pm.Id,
                    AccountId = pm.AccountId,
                    ProjectId = pm.ProjectId,
                    JoinedAt = pm.JoinedAt,
                    InvitedAt = pm.InvitedAt,
                    Status = pm.Status,
                    FullName = pm.Account?.FullName, 
                    Username = pm.Account?.Username, 
                    Picture = pm.Account?.Picture,  
                    ProjectPositions = pm.ProjectPosition?.Select(pp => new ProjectPositionResponseDTO
                    {
                        Id = pp.Id,
                        ProjectMemberId = pp.ProjectMemberId,
                        Position = pp.Position
                    }).ToList()
                }).ToList()
            };

            return details;
        }

    }
}
