using AutoMapper;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.TaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Services.SprintServices
{
    public class SprintService : ISprintService
    {
        private readonly IMapper _mapper;
        private readonly ISprintRepository _repo;
        private readonly ITaskService _taskService;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<SprintService> _logger;

        public SprintService(IMapper mapper, ISprintRepository repo, ILogger<SprintService> logger, ITaskService taskService, ITaskRepository taskRepo, IProjectRepository projectRepo)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _taskService = taskService;
            _taskRepo = taskRepo;
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


        public async Task<string> GenerateSprintNameAsync(int projectId, string projectKey)
        {
            var entities = await _repo.GetByProjectIdDescendingAsync(projectId);

            int nextNumber = 1;

            if (entities.Any())
            {
                string latestName = entities[0].Name?.Trim() ?? "";
                var parts = latestName.Split(' ');
                string lastPart = parts.LastOrDefault();

                if (int.TryParse(lastPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;

                    string prefix = string.Join(" ", parts.Take(parts.Length - 1));
                    return $"{prefix} {nextNumber}";
                }
                else
                {
                    return $"{latestName} 1";
                }
            }
            return $"{projectKey} 1";
        }



        public async Task<SprintResponseDTO> CreateSprintQuickAsync(SprintQuickRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var projectEntity = await _projectRepo.GetProjectByKeyAsync(request.projectKey);

            if (projectEntity == null)
                throw new Exception($"Project with key '{request.projectKey}' not found.");

            var entity = new Sprint();
            entity.ProjectId = projectEntity.Id;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Name = await GenerateSprintNameAsync(projectEntity.Id, projectEntity.ProjectKey);
            entity.Status = "FUTURE";


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

            entity.Status = request.Status;
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.Status.Equals("ACTIVE"))
            {
                entity.StartDate = DateTime.UtcNow;
                if (entity.EndDate == null && entity.PlannedEndDate != null)
                {
                    entity.EndDate = entity.PlannedEndDate;
                }

            }
            else if (request.Status.Equals("COMPLETED"))
            {
                entity.EndDate = DateTime.UtcNow;
                if (entity.StartDate == null && entity.PlannedStartDate != null)
                {
                    entity.StartDate = entity.PlannedStartDate;
                }
            }

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


        public async Task DeleteSprintWithTask(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            var taskEntities = await _taskRepo.GetBySprintIdAsync(id);


            try
            {


                if (taskEntities != null)
                {
                    foreach (var task in taskEntities)
                    {
                        task.SprintId = null;
                        await _taskRepo.Update(task);
                    }
                }

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

            if (status.Equals("ACTIVE"))
            {
                entity.StartDate = DateTime.UtcNow;
                if (entity.EndDate == null && entity.PlannedEndDate != null)
                {
                    entity.EndDate = entity.PlannedEndDate;
                }

            }
            else if (status.Equals("COMPLETED"))
            {
                entity.EndDate = DateTime.UtcNow;
                if (entity.StartDate == null && entity.PlannedStartDate != null)
                {
                    entity.StartDate = entity.PlannedStartDate;
                }
            }


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


        public async Task<(bool IsValid, string Message)> CheckSprintDatesAsync(string projectKey, DateTime checkStartDate)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            var dateWithinProject = await IsSprintWithinProject(projectKey, checkStartDate);

            if (!dateWithinProject)
                return (false, "current date is not in project");

            var entities = await _repo.GetByProjectIdDescendingAsync(project.Id);
            if (entities == null || !entities.Any())
                return (true, "Valid date");

            var latestSprint = entities[0];
            if (!latestSprint.EndDate.HasValue)
            {
                if (latestSprint.PlannedEndDate.HasValue)
                {
                    if (checkStartDate > latestSprint.PlannedEndDate.Value)
                        return (true, "Start date is valid.");
                    return (false, $"Start date must be after planned end date ({latestSprint.PlannedEndDate.Value}).");
                }
                return (true, "No end date or planned end date set for the latest sprint.");
            }

            if (checkStartDate > latestSprint.EndDate.Value)
                return (true, "Start date is valid.");
            return (false, $"Start date must be after end date ({latestSprint.EndDate.Value}).");
        }



        public async Task<bool> IsSprintWithinProject(string projectKey, DateTime checkSprintDate)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            if (!project.StartDate.HasValue || !project.EndDate.HasValue)
                throw new ArgumentException($"Project '{projectKey}' has invalid start or end date.");

            if (project.StartDate.Value >= project.EndDate.Value)
                throw new ArgumentException($"Project '{projectKey}' has invalid date range: start date must be before end date.");

            return checkSprintDate > project.StartDate.Value && checkSprintDate < project.EndDate.Value;
        }


        public async Task<string> MoveTaskToSprint(int sprintOldId, int sprintNewId, string type)
        {
            var validTypes = new[] { "BACKLOG", "CHANGE", "NEW_SPRINT" };
            if (!validTypes.Contains(type))
                throw new ArgumentException($"Invalid type '{type}'. Must be one of: BACKLOG, CHANGE, NEW_SPRINT.");

            var sprintOld = await _repo.GetByIdAsync(sprintOldId);
            if (sprintOld == null)
                throw new KeyNotFoundException($"Sprint with id '{sprintOldId}' not found.");

            int? sprintNewIdNull = sprintNewId;

            if (type.Equals("CHANGE", StringComparison.OrdinalIgnoreCase))
            {
                var sprintNew = await _repo.GetByIdAsync(sprintNewId);
                if (sprintNew == null)
                    throw new KeyNotFoundException($"Sprint with id '{sprintNewId}' not found.");
            }
            else if (type.Equals("NEW_SPRINT", StringComparison.OrdinalIgnoreCase))
            {
                var projectEntity = await _projectRepo.GetByIdAsync(sprintOld.ProjectId);
                if (projectEntity == null)
                    throw new KeyNotFoundException($"Project with Id '{sprintOld.ProjectId}' not found.");

                SprintQuickRequestDTO sprintQuickRequestDTO = new SprintQuickRequestDTO
                {
                    projectKey = projectEntity.ProjectKey
                };

                var sprint = await CreateSprintQuickAsync(sprintQuickRequestDTO);
                sprintNewIdNull = sprint.Id;
            }
            else if (type.Equals("BACKLOG", StringComparison.OrdinalIgnoreCase))
            {
                sprintNewIdNull = null;
            }

            var tasksToMove = await _taskRepo.GetBySprintIdAsync(sprintOldId);
            if (tasksToMove == null || !tasksToMove.Any())
                throw new KeyNotFoundException($"No tasks found in sprint with id '{sprintOldId}'.");

            int movedCount = 0;
            foreach (var task in tasksToMove)
            {
                if (!string.Equals(task.Status, "DONE", StringComparison.OrdinalIgnoreCase))
                {
                    task.SprintId = sprintNewIdNull;
                    await _taskRepo.Update(task);
                    movedCount++;
                }
            }

            return $"Moved {movedCount} task(s) from Sprint {sprintOldId} to {(sprintNewIdNull.HasValue ? $"Sprint {sprintNewIdNull}" : "Backlog")}.";
        }





    }
}
