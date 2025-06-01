using AutoMapper;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
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
        private readonly IProjectRepository _repo;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IMapper mapper, IProjectRepository repo, ILogger<ProjectService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<ProjectResponseDTO>> GetAllProjects()
        {
            var entities = await _repo.GetAllProjects();
            return _mapper.Map<List<ProjectResponseDTO>>(entities);
        }

        public async Task<ProjectResponseDTO> GetProjectById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task<List<ProjectResponseDTO>> SearchProjects(string searchTerm, string? projectType, string? status)
        {
            if (string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(projectType) && string.IsNullOrEmpty(status))
                throw new ArgumentException("At least one search criterion must be provided.");

            var entities = await _repo.SearchProjects(searchTerm, projectType, status);
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
                await _repo.Add(entity);
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
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian chỉnh sửa

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update project: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task DeleteProject(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project: {ex.Message}", ex);
            }
        }
    }
}
