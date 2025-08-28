using AutoMapper;
using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.Sprint;
using IntelliPM.Data.Enum.Task;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.AiServices.SprintPlanningServices;
using IntelliPM.Services.TaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

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
            return $"{projectKey} Sprint 1";
        }



        public async Task<List<SprintResponseDTO>> CreateSprintAndAddTaskAsync(string projectKey, List<SprintWithTasksDTO> requests)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.", nameof(projectKey));

            if (requests == null || !requests.Any())
                throw new ArgumentNullException(nameof(requests), "Request list cannot be null or empty.");

            var projectEntity = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (projectEntity == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            var projectStart = projectEntity.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var projectEnd = projectEntity.EndDate?.ToUniversalTime() ?? projectStart.AddMonths(6);

            var createdSprints = new List<SprintResponseDTO>();

            foreach (SprintWithTasksDTO sprint in requests)
            {
                // Ensure UTC for sprint dates
                var startDate = sprint.StartDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(sprint.StartDate, DateTimeKind.Utc)
                    : sprint.StartDate.ToUniversalTime();
                var endDate = sprint.EndDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(sprint.EndDate, DateTimeKind.Utc)
                    : sprint.EndDate.ToUniversalTime();

                // Validate sprint dates against project timeline
                if (startDate < projectStart || endDate > projectEnd)
                {
                    _logger.LogWarning(
                        "Invalid sprint dates for '{SprintTitle}': StartDate={StartDate:yyyy-MM-dd}, EndDate={EndDate:yyyy-MM-dd}, ProjectTimeline={ProjectStart:yyyy-MM-dd} to {ProjectEnd:yyyy-MM-dd}",
                        sprint.Title, startDate, endDate, projectStart, projectEnd);
                    throw new ArgumentException(
                        $"Invalid sprint dates for '{sprint.Title}': Dates ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}) are not within project timeline ({projectStart:yyyy-MM-dd} to {projectEnd:yyyy-MM-dd}).");
                }

                // Validate sprint dates with CheckSprintDatesAsync
                var (isValid, message) = await CheckSprintDatesAsync(projectKey, startDate, endDate);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid sprint dates for '{SprintTitle}': {Message}, StartDate={StartDate:yyyy-MM-dd}, EndDate={EndDate:yyyy-MM-dd}",
                        sprint.Title, message, startDate, endDate);
                    throw new ArgumentException($"Invalid sprint dates for '{sprint.Title}': {message}");
                }

                var entity = new Sprint
                {
                    ProjectId = projectEntity.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Name = await GenerateSprintNameAsync(projectEntity.Id, projectEntity.ProjectKey),
                    Goal = sprint.Title,
                    Status = SprintStatusEnum.FUTURE.ToString(),
                    StartDate = startDate,
                    EndDate = endDate,
                };

                try
                {
                    await _repo.Add(entity);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Failed to create sprint '{SprintName}' due to database error.", entity.Name);
                    throw new Exception($"Failed to create sprint '{entity.Name}' due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
                }

                // Add tasks to sprint
                if (sprint.Tasks != null && sprint.Tasks.Any())
                {
                    foreach (SprintTaskDTO task in sprint.Tasks)
                    {
                        var taskEntity = await _taskRepo.GetByIdAsync(task.TaskId);
                        if (taskEntity == null)
                        {
                            _logger.LogWarning("Task with ID '{TaskId}' not found for sprint '{SprintName}'.", task.TaskId, entity.Name);
                            throw new KeyNotFoundException($"Task with ID '{task.TaskId}' not found.");
                        }

                        taskEntity.UpdatedAt = DateTime.UtcNow;
                        taskEntity.SprintId = entity.Id;
                        taskEntity.Priority = task.Priority;
                        taskEntity.PlannedHours = task.PlannedHours;

                        try
                        {
                            await _taskRepo.Update(taskEntity);
                        }
                        catch (DbUpdateException ex)
                        {
                            _logger.LogError(ex, "Failed to assign task '{TaskId}' to sprint '{SprintName}'.", task.TaskId, entity.Name);
                            throw new Exception($"Failed to assign task '{task.TaskId}' to sprint '{entity.Name}': {ex.InnerException?.Message ?? ex.Message}", ex);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No tasks provided for sprint '{SprintName}'.", entity.Name);
                }

                createdSprints.Add(_mapper.Map<SprintResponseDTO>(entity));
            }

            return createdSprints;
        }

        public async Task<(bool IsValid, string Message)> CheckSprintDatesAsync(string projectKey, DateTime checkStartDate, DateTime checkEndDate)
        {
            checkStartDate = checkStartDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(checkStartDate, DateTimeKind.Utc)
                : checkStartDate.ToUniversalTime();
            checkEndDate = checkEndDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(checkEndDate, DateTimeKind.Utc)
                : checkEndDate.ToUniversalTime();

            if (checkStartDate >= checkEndDate)
            {
                _logger.LogWarning("Start date ({StartDate:yyyy-MM-dd}) is not before end date ({EndDate:yyyy-MM-dd}).", checkStartDate, checkEndDate);
                return (false, $"Start date ({checkStartDate:yyyy-MM-dd}) must be before end date ({checkEndDate:yyyy-MM-dd}).");
            }

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
            {
                _logger.LogError("Project with key '{ProjectKey}' not found.", projectKey);
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");
            }

            var projectStart = project.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var projectEnd = project.EndDate?.ToUniversalTime() ?? projectStart.AddMonths(6);

            // Explicitly check project timeline
            if (checkStartDate < projectStart || checkEndDate > projectEnd)
            {
                return (false, $"Sprint dates are not within project timeline (Start: {projectStart:yyyy-MM-dd}, End: {projectEnd:yyyy-MM-dd}).");
            }

            // Check for overlaps with existing sprints
            var existingSprints = await _repo.GetByProjectIdDescendingAsync(project.Id);
            if (existingSprints == null || !existingSprints.Any())
            {
                _logger.LogInformation("No existing sprints for project '{ProjectKey}'. Dates are valid.", projectKey);
                return (true, "Valid date: No existing sprints.");
            }

            foreach (var sprint in existingSprints)
            {
                if (!sprint.StartDate.HasValue || !sprint.EndDate.HasValue)
                {
                    _logger.LogWarning("Sprint '{SprintName}' has null dates. Skipping overlap check.", sprint.Name);
                    continue;
                }

                var sprintStart = sprint.StartDate.Value.ToUniversalTime();
                var sprintEnd = sprint.EndDate.Value.ToUniversalTime();

                // Check for overlap
                if (checkStartDate <= sprintEnd && checkEndDate >= sprintStart)
                {

                    return (false, $"Sprint dates overlap with existing sprint '{sprint.Name}' (Start: {sprintStart:yyyy-MM-dd}, End: {sprintEnd:yyyy-MM-dd}).");
                }
            }

            _logger.LogInformation("Sprint dates (Start: {StartDate:yyyy-MM-dd}, End: {EndDate:yyyy-MM-dd}) are valid for project '{ProjectKey}'.", checkStartDate, checkEndDate, projectKey);
            return (true, "Sprint dates are valid.");
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
            entity.Status = SprintStatusEnum.FUTURE.ToString();


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

            if (request.Status.Equals(SprintStatusEnum.ACTIVE.ToString()))
            {
                entity.StartDate = DateTime.UtcNow;
                if (entity.EndDate == null && entity.PlannedEndDate != null)
                {
                    entity.EndDate = entity.PlannedEndDate;
                }

            }
            else if (request.Status.Equals(SprintStatusEnum.COMPLETED.ToString()))
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

            if (status.Equals(SprintStatusEnum.ACTIVE.ToString()))
            {
                entity.StartDate = DateTime.UtcNow;
                if (entity.EndDate == null && entity.PlannedEndDate != null)
                {
                    entity.EndDate = entity.PlannedEndDate;
                }

            }
            else if (status.Equals(SprintStatusEnum.COMPLETED.ToString()))
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


        public async Task<List<SprintResponseDTO>> GetSprintByProjectIdDescending(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _repo.GetByProjectIdDescendingAsync(projectId);

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



        public async Task<(bool IsValid, string Message)> CheckActiveSprintStartDateAsync(string projectKey, DateTime checkStartDate, DateTime checkEndDate, int activeSprintId)
        {
            checkStartDate = checkStartDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(checkStartDate, DateTimeKind.Utc)
                : checkStartDate.ToUniversalTime();
            checkEndDate = checkEndDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(checkEndDate, DateTimeKind.Utc)
                : checkEndDate.ToUniversalTime();

            if (checkStartDate >= checkEndDate)
            {
                return (false, $"Start date ({checkStartDate:yyyy-MM-dd}) must be before end date ({checkEndDate:yyyy-MM-dd}).");
            }

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");
            }

            var activeSprint = await _repo.GetByIdAsync(activeSprintId);
            if (activeSprint == null || activeSprint.ProjectId != project.Id)
            {
                throw new KeyNotFoundException($"Active sprint with ID '{activeSprintId}' not found or does not belong to project '{projectKey}'.");
            }

            var projectStart = project.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var projectEnd = project.EndDate?.ToUniversalTime() ?? projectStart.AddMonths(6);

            if (checkStartDate < projectStart || checkEndDate > projectEnd)
            {
                return (false, $"Sprint dates ({checkStartDate:yyyy-MM-dd} to {checkEndDate:yyyy-MM-dd}) must be within project timeline (Start: {projectStart:yyyy-MM-dd}, End: {projectEnd:yyyy-MM-dd}).");
            }

            var existingSprints = await _repo.GetByProjectIdDescendingAsync(project.Id);
            if (existingSprints == null || !existingSprints.Any())
            {
                return (true, "Valid dates: No existing sprints.");
            }

            foreach (var sprint in existingSprints)
            {
                if (sprint.Id == activeSprintId)
                {
                    continue;
                }

                if (!sprint.StartDate.HasValue || !sprint.EndDate.HasValue)
                {
                    continue;
                }

                var sprintStart = sprint.StartDate.Value.ToUniversalTime();
                var sprintEnd = sprint.EndDate.Value.ToUniversalTime();

                if (checkStartDate <= sprintEnd && checkEndDate >= sprintStart)
                {
                    return (false, $"Sprint dates ({checkStartDate:yyyy-MM-dd} to {checkEndDate:yyyy-MM-dd}) overlap with existing sprint '{sprint.Name}' (Start: {sprintStart:yyyy-MM-dd}, End: {sprintEnd:yyyy-MM-dd}).");
                }
            }

            return (true, "Sprint dates are valid.");
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
                if (!string.Equals(task.Status, TaskStatusEnum.DONE.ToString() , StringComparison.OrdinalIgnoreCase))
                {
                    task.SprintId = sprintNewIdNull;
                    await _taskRepo.Update(task);
                    movedCount++;
                }
            }

            return $"Moved {movedCount} task(s) from Sprint {sprintOldId} to {(sprintNewIdNull.HasValue ? $"Sprint {sprintNewIdNull}" : "Backlog")}.";
        }


        public async Task<SprintResponseDTO> GetActiveSprintWithTasksByProjectKeyAsync(string projectKey)
        {
            if (string.IsNullOrWhiteSpace(projectKey))
                throw new ArgumentException("Project key is required.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            var activeSprint = await _repo.GetActiveSprintByProjectIdAsync(project.Id);
            if (activeSprint == null)
                throw new KeyNotFoundException($"No active sprint found for Project '{projectKey}'.");

            return _mapper.Map<SprintResponseDTO>(activeSprint);
        }


    }
}
