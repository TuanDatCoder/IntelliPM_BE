using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Services.TaskServices;
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
        private readonly ITaskService _taskService;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<SprintService> _logger;

        public SprintService(IMapper mapper, ISprintRepository repo, ILogger<SprintService> logger, ITaskService taskService, IProjectRepository projectRepo)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _taskService = taskService;
            _projectRepo = projectRepo;
         
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


        public async Task<SprintResponseDTO> CreateSprintQuick(SprintQuickRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");


            var entity = _mapper.Map<Sprint>(request);
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;



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
            entity.UpdatedAt = DateTime.UtcNow; 

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
            entity.UpdatedAt = DateTime.UtcNow;

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

        public async Task<List<SprintResponseDTO>> GetSprintByProjectId(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _repo.GetByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No sprints found for Project ID {projectId}.");

            return _mapper.Map<List<SprintResponseDTO>>(entities);
        }

        public async Task<List<SprintWithTaskListResponseDTO>> GetSprintsByProjectKeyWithTasksAsync(string projectKey)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
                throw new ArgumentException("Project key is required.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");
            var entities = await _repo.GetByProjectIdAsync(project.Id);
            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No sprints found for Project '{projectKey}'.");

            var dtos = new List<SprintWithTaskListResponseDTO>();
            foreach (var entity in entities)
            {
                var sprintDto = _mapper.Map<SprintWithTaskListResponseDTO>(entity);
                var tasks = await _taskService.GetTasksBySprintIdAsync(entity.Id); 
                sprintDto.Tasks = tasks; 
                dtos.Add(sprintDto);
            }

            return dtos;
        }


    }
}
