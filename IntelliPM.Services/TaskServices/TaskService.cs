using AutoMapper;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ActivityLogActionType;
using IntelliPM.Data.Enum.ActivityLogRelatedEntityType;
using IntelliPM.Data.Enum.Epic;
using IntelliPM.Data.Enum.Task;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Services.TaskCommentServices;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using IntelliPM.Services.WorkLogServices;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Services.TaskServices
{
    public class TaskService : ITaskService
    {
        private readonly IMapper _mapper;
        private readonly ITaskRepository _taskRepo;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ITaskCommentService _taskCommentService;
        private readonly IWorkItemLabelService _workItemLabelService;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly ITaskDependencyRepository _taskDependencyRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IWorkLogService _workLogService;
        private readonly IActivityLogService _activityLogService;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly IGeminiService _geminiService;
        private readonly IServiceProvider _serviceProvider;
        private readonly object _idGenerationLock = new object();
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public TaskService(IMapper mapper, ITaskRepository taskRepo, IEpicRepository epicRepo, IProjectRepository projectRepo, ISubtaskRepository subtaskRepo, IAccountRepository accountRepo, ITaskCommentService taskCommentService, IWorkItemLabelService workItemLabelService, ITaskAssignmentRepository taskAssignmentRepository, ITaskDependencyRepository taskDependencyRepo, IProjectMemberRepository projectMemberRepo, IDynamicCategoryRepository dynamicCategoryRepo, IWorkLogService workLogService, ISprintRepository sprintRepo, IActivityLogService activityLogService, IMilestoneRepository milestoneRepo, IGeminiService geminiService, IServiceProvider serviceProvider, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _mapper = mapper;
            _taskRepo = taskRepo;
            _epicRepo = epicRepo;
            _projectRepo = projectRepo;
            _subtaskRepo = subtaskRepo;
            _accountRepo = accountRepo;
            _taskCommentService = taskCommentService;
            _workItemLabelService = workItemLabelService;
            _taskAssignmentRepo = taskAssignmentRepository;
            _taskDependencyRepo = taskDependencyRepo;
            _projectMemberRepo = projectMemberRepo;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _workLogService = workLogService;
            _sprintRepo = sprintRepo;
            _activityLogService = activityLogService;
            _milestoneRepo = milestoneRepo;
            _geminiService = geminiService;
            _serviceProvider = serviceProvider;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }
        public async Task<List<TaskResponseDTO>> GenerateTaskPreviewAsync(int projectId)
        {
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_status");

            var suggestions = await _geminiService.GenerateTaskAsync(project.Description);

            if (suggestions == null || !suggestions.Any())
                return new List<TaskResponseDTO>();

            var now = DateTime.UtcNow;

            var tasks = suggestions.Select(s => new Tasks
            {
                ProjectId = projectId,
                Title = s.Title?.Trim() ?? "Untitled Task",
                Description = s.Description?.Trim() ?? string.Empty,
                Type = s.Type?.Trim() ?? string.Empty,
                Status = TaskStatusEnum.TO_DO.ToString(),
                ManualInput = false,
                GenerationAiInput = true,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            return _mapper.Map<List<TaskResponseDTO>>(tasks);
        }

        public async Task<List<TaskResponseDTO>> GenerateTaskPreviewByEpicAsync(string epicId)
        {
            var project = await _epicRepo.GetByIdAsync(epicId);
            if (project == null)
                throw new KeyNotFoundException($"Epic with ID {epicId} not found.");

            var suggestions = await _geminiService.GenerateTaskAsync(project.Description);

            if (suggestions == null || !suggestions.Any())
                return new List<TaskResponseDTO>();
            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_status");
            var now = DateTime.UtcNow;

            var tasks = suggestions.Select(s => new Tasks
            {
                EpicId = epicId,
                Title = s.Title?.Trim() ?? "Untitled Task",
                Description = s.Description?.Trim() ?? string.Empty,
                Type = s.Type?.Trim() ?? string.Empty,
                Status = dynamicStatus,
                ManualInput = false,
                GenerationAiInput = true,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            return _mapper.Map<List<TaskResponseDTO>>(tasks);
        }

        public async Task<List<TaskResponseDTO>> GetAllTasks()
        {
            var entities = await _taskRepo.GetAllTasks();
            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> GetTaskById(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var dto = _mapper.Map<TaskResponseDTO>(entity);

            var isInProgress = entity.Status.Equals(TaskStatusEnum.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase);
            var isDone = entity.Status.Equals(TaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase);

            // Bổ sung check trước khi cập nhật trạng thái
            var dependencies = await _taskDependencyRepo
                .FindAllAsync(d => d.LinkedTo == id && d.ToType == "Task");

            var warnings = new List<string>();

            foreach (var dep in dependencies)
            {
                object? source = null;
                string? sourceStatus = null;

                switch (dep.FromType)
                {
                    case "task":
                        var task = await _taskRepo.GetByIdAsync(dep.LinkedFrom);
                        if (task == null) continue;
                        source = task;
                        sourceStatus = task.Status;
                        break;

                    case "subtask":
                        var subtask = await _subtaskRepo.GetByIdAsync(dep.LinkedFrom);
                        if (subtask == null) continue;
                        source = subtask;
                        sourceStatus = subtask.Status;
                        break;

                    case "milestone":
                        var milestone = await _milestoneRepo.GetByKeyAsync(dep.LinkedFrom);
                        if (milestone == null) continue;
                        source = milestone;
                        sourceStatus = milestone.Status;
                        break;

                    default:
                        continue;
                }

                switch (dep.Type.ToUpper())
                {
                    case "FINISH_START":
                        // Task chỉ có thể bắt đầu (IN_PROGRESS hoặc DONE) nếu LinkedFrom đã hoàn thành (DONE)
                        if ((isInProgress || isDone) && !string.Equals(sourceStatus, "DONE", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before starting.");
                        }
                        break;

                    case "START_START":
                        // Task chỉ có thể bắt đầu (IN_PROGRESS hoặc DONE) nếu LinkedFrom đã bắt đầu (IN_PROGRESS hoặc DONE)
                        if ((isInProgress || isDone) && string.Equals(sourceStatus, "TO_DO", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be started (IN_PROGRESS or DONE) before starting.");
                        }
                        break;


                    case "FINISH_FINISH":
                        // Task chỉ có thể hoàn thành (DONE) nếu LinkedFrom đã hoàn thành (DONE)
                        if (isDone && !string.Equals(sourceStatus, "DONE", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed (DONE) before it can be completed.");
                        }
                        break;

                    case "START_FINISH":
                        // Task chỉ có thể hoàn thành (DONE) nếu LinkedFrom đã bắt đầu (IN_PROGRESS hoặc DONE)
                        if (isDone && string.Equals(sourceStatus, "TO_DO", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' can only be completed after {dep.FromType.ToLower()} '{dep.LinkedFrom}' has started (IN_PROGRESS or DONE).");
                        }
                        break;
                }
            }

            dto.Warnings = warnings;
            return dto;
        }

        public async Task<List<TaskResponseDTO>> GetTaskByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entities = await _taskRepo.GetByTitleAsync(title);
            if (!entities.Any())
                throw new KeyNotFoundException($"No tasks found with title containing '{title}'.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> CreateTask(TaskRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Task title is required.", nameof(request.Title));

            if (request.ProjectId <= 0)
                throw new ArgumentException("Project ID is required and must be greater than 0.", nameof(request.ProjectId));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            var projectKey = await _projectRepo.GetProjectKeyAsync(request.ProjectId);
            if (string.IsNullOrEmpty(projectKey))
                throw new InvalidOperationException($"Invalid project key for Project ID {request.ProjectId}.");

            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_status");
            var dynamicPriority = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_priority");
           // var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "CREATE");
            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.CREATE.ToString();

            var entity = _mapper.Map<Tasks>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
            entity.Priority = dynamicPriority;
            entity.Status = dynamicStatus;

            try
            {
                await _taskRepo.Add(entity);
                await RecalculateTaskPlannedHours(entity.Id);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Created task '{entity.Id}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });

                if (!string.IsNullOrEmpty(entity.EpicId))
                {
                    var epic = await _epicRepo.GetByIdAsync(entity.EpicId);
                    if (epic != null && epic.Status.Equals(EpicStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        epic.Status = EpicStatusEnum.IN_PROGRESS.ToString();
                        epic.UpdatedAt = DateTime.UtcNow;
                        await _epicRepo.Update(epic);
                    }
                }

                //await CalculatePlannedHoursAsync(entity.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }

            catch (Exception ex)
            {
                throw new Exception($"Failed to create task: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        private async Task RecalculateTaskPlannedHours(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return;

            var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);
            var assignedAccountIds = taskAssignments.Select(a => a.AccountId).Distinct().ToList();

            var projectMembers = new List<ProjectMember>();

            foreach (var accountId in assignedAccountIds)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, task.ProjectId);
                if (member != null && member.WorkingHoursPerDay.HasValue)
                {
                    decimal hourlyRate = member.HourlyRate ?? 0m;
                    projectMembers.Add(new ProjectMember
                    {
                        Id = member.Id,
                        AccountId = member.AccountId,
                        ProjectId = member.ProjectId,
                        WorkingHoursPerDay = member.WorkingHoursPerDay,
                        HourlyRate = hourlyRate,
                    });
                }
            }

            decimal totalWorkingHoursPerDay = projectMembers.Sum(m => m.WorkingHoursPerDay.Value);
            decimal? totalCost = 0m;

            if (totalWorkingHoursPerDay > 0)
            {
                foreach (var member in projectMembers)
                {
                    var memberAssignedHours = task.PlannedHours * (member.WorkingHoursPerDay.Value / totalWorkingHoursPerDay);
                    var memberCost = memberAssignedHours * member.HourlyRate.Value;
                    totalCost += memberCost;

                    var taskAssignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(taskId, member.AccountId);
                    if (taskAssignment != null)
                    {
                        taskAssignment.PlannedHours = memberAssignedHours;
                        await _taskAssignmentRepo.Update(taskAssignment);
                    }
                }

                task.PlannedResourceCost = totalCost;
                //task.PlannedCost = totalCost;
            }
            else
            {
                // Warning: No assignments or working hours, costs remain 0
                task.PlannedResourceCost = 0m;
                //task.PlannedCost = 0m;
            }

            await _taskRepo.Update(task);
            //await UpdateTaskProgressAsync(task);
        }

        public async Task<List<TaskResponseDTO>> CreateTasksBulkAsync(List<TaskRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
            {
                throw new ArgumentException("Request list cannot be null or empty.");
            }

            var results = new List<TaskResponseDTO>();

            foreach (var request in requests)
            {

                var result = await CreateTask(request);
                results.Add(result);
            }

            return results;
        }

        public async Task<TaskResponseDTO> UpdateTask(string id, TaskRequestDTO request)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                //await CalculatePlannedHoursAsync(entity.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task: {ex.Message}", ex);
            }

            //if (request.Dependencies != null)
            //{
            //    await _taskDependencyRepo.DeleteByTaskIdAsync(id);

            //    var newDeps = request.Dependencies.Select(d => new TaskDependency
            //    {
            //        FromType = d.FromType,
            //        LinkedFrom = d.LinkedFrom,
            //        ToType = d.ToType,
            //        LinkedTo = d.LinkedTo,
            //        Type = d.Type
            //    }).ToList();

            //    // Lưu lại
            //    await _taskDependencyRepo.AddRangeAsync(newDeps);
            //}

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskUpdateResponseDTO> UpdateTaskTrue(string id, TaskUpdateRequestDTO request)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task: {ex.Message}", ex);
            }


            return _mapper.Map<TaskUpdateResponseDTO>(entity);
        }

        public async Task DeleteTask(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            try
            {
                await _taskRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task: {ex.Message}", ex);
            }
        }

        public async Task<TaskResponseDTO> ChangeTaskStatus(string id, string status, int createdBy)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var validStatuses = await _dynamicCategoryHelper.GetTaskStatusesAsync();
            if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid status: '{status}'. Must be one of: {string.Join(", ", validStatuses)}");

            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "STATUS_CHANGE");

            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.STATUS_CHANGE.ToString();

            var isInProgress = status.Equals(TaskStatusEnum.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase);
            var isDone = status.Equals(TaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase);

            if (isInProgress)
                entity.ActualStartDate = DateTime.UtcNow;
            if (isDone)
                entity.ActualEndDate = DateTime.UtcNow;

            // Bổ sung check trước khi cập nhật trạng thái
            var dependencies = await _taskDependencyRepo
                .FindAllAsync(d => d.LinkedTo == id && d.ToType == "Task");

            var warnings = new List<string>();

            foreach (var dep in dependencies)
            {
                object? source = null;
                string? sourceStatus = null;

                switch (dep.FromType)
                {
                    case "task":
                        var task = await _taskRepo.GetByIdAsync(dep.LinkedFrom);
                        if (task == null) continue;
                        source = task;
                        sourceStatus = task.Status;
                        break;

                    case "subtask":
                        var subtask = await _subtaskRepo.GetByIdAsync(dep.LinkedFrom);
                        if (subtask == null) continue;
                        source = subtask;
                        sourceStatus = subtask.Status;
                        break;

                    case "milestone":
                        var milestone = await _milestoneRepo.GetByKeyAsync(dep.LinkedFrom);
                        if (milestone == null) continue;
                        source = milestone;
                        sourceStatus = milestone.Status;
                        break;

                    default:
                        continue;
                }

                switch (dep.Type.ToUpper())
                {
                    case "FINISH_START":
                        // Task chỉ có thể bắt đầu (IN_PROGRESS hoặc DONE) nếu LinkedFrom đã hoàn thành (DONE)
                        if ((isInProgress || isDone) && !string.Equals(sourceStatus, "DONE", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before starting.");
                        }
                        break;

                    case "START_START":
                        // Task chỉ có thể bắt đầu (IN_PROGRESS hoặc DONE) nếu LinkedFrom đã bắt đầu (IN_PROGRESS hoặc DONE)
                        if ((isInProgress || isDone) && string.Equals(sourceStatus, "TO_DO", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be started (IN_PROGRESS or DONE) before starting.");
                        }
                        break;


                    case "FINISH_FINISH":
                        // Task chỉ có thể hoàn thành (DONE) nếu LinkedFrom đã hoàn thành (DONE)
                        if (isDone && !string.Equals(sourceStatus, "DONE", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed (DONE) before it can be completed.");
                        }
                        break;

                    case "START_FINISH":
                        // Task chỉ có thể hoàn thành (DONE) nếu LinkedFrom đã bắt đầu (IN_PROGRESS hoặc DONE)
                        if (isDone && string.Equals(sourceStatus, "TO_DO", StringComparison.OrdinalIgnoreCase))
                        {
                            warnings.Add($"Task '{id}' can only be completed after {dep.FromType.ToLower()} '{dep.LinkedFrom}' has started (IN_PROGRESS or DONE).");
                        }
                        break;
                }
            }

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            //await UpdateTaskProgressAsync(entity);

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed status of task '{entity.Id}' to '{status}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });

                if (isInProgress)
                {
                    await _workLogService.GenerateDailyWorkLogsAsync();
                }

                var parentEpicId = entity.EpicId;
                var allTasks = await _taskRepo.GetByEpicIdAsync(parentEpicId);

                var parentEpic = await _epicRepo.GetByIdAsync(parentEpicId);
                if (parentEpic != null)
                {
                    if (allTasks.All(st => st.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase)))
                    {
                        parentEpic.Status = "DONE";
                        parentEpic.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (allTasks.Any(st => st.Status.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase)))
                    {
                        parentEpic.Status = "IN_PROGRESS";
                        parentEpic.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {

                    }
                    parentEpic.UpdatedAt = DateTime.UtcNow;
                    await _epicRepo.Update(parentEpic);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task status: {ex.Message}", ex);
            }

            //return _mapper.Map<TaskResponseDTO>(entity);
            var result = _mapper.Map<TaskResponseDTO>(entity);
            result.Warnings = warnings;
            return result;
        }

        //private async Task UpdateTaskProgressAsync(Tasks task)
        //{
        //    if (task.Subtask?.Any() ?? false)
        //    {
        //        var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(task.Id);
        //        task.PercentComplete = (int)Math.Round(subtasks.Average(st => st.PercentComplete ?? 0));
        //    }
        //    else
        //    {
        //        if (task.Status == "DONE") task.PercentComplete = 100;
        //        else if (task.Status == "TO_DO") task.PercentComplete = 0;
        //        else if (task.Status == "IN_PROGRESS" && task.PlannedHours.HasValue && task.PlannedHours.Value > 0)
        //        {
        //            var rawProgress = ((task.ActualHours ?? 0) / task.PlannedHours.Value) * 100;
        //            task.PercentComplete = Math.Min((int)rawProgress, 99);
        //        }
        //        else task.PercentComplete = 0;
        //    }

        //    task.UpdatedAt = DateTime.UtcNow;
        //    await _taskRepo.Update(task);
        //}

        public async Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _taskRepo.GetByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No tasks found for Project ID {projectId}.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<List<TaskResponseDTO>> GetTasksByEpicIdAsync(string epicId)
        {

            var entities = await _taskRepo.GetByEpicIdAsync(epicId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No tasks found for epic ID {epicId}.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }


        public async Task<TaskResponseDTO> ChangeTaskType(string id, string type, int createdBy)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var validTypes = await _dynamicCategoryHelper.GetTaskTypesAsync();
            if (!validTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid type: '{type}'. Must be one of: {string.Join(", ", validTypes)}");

           // var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            entity.Type = type;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed type of task '{entity.Id}' to '{type}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task type: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPriority(string id, string priority, int createdBy)
        {
            if (string.IsNullOrEmpty(priority))
                throw new ArgumentException("Priority cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var validPriorities = await _dynamicCategoryHelper.GetTaskPrioritiesAsync();
            if (!validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid priority: '{priority}'. Must be one of: {string.Join(", ", validPriorities)}");

            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");


            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            entity.Priority = priority;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed priority of task '{entity.Id}' to '{priority}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change priority: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskReporter(string id, int reporter, int createdBy)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");


            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            entity.ReporterId = reporter;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed reporter of task '{entity.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change reporter: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }
        public async Task<TaskDetailedResponseDTO> GetTaskByIdDetailed(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var dto = _mapper.Map<TaskDetailedResponseDTO>(entity);
            await EnrichTaskDetailedResponse(dto);
            return dto;
        }

        public async Task<List<TaskDetailedResponseDTO>> GetTasksByProjectIdDetailed(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _taskRepo.GetByProjectIdAsync(projectId);

            var dtos = _mapper.Map<List<TaskDetailedResponseDTO>>(entities);
            foreach (var dto in dtos)
            {
                await EnrichTaskDetailedResponse(dto);
            }

            return dtos;
        }
        private async Task EnrichTaskDetailedResponse(TaskDetailedResponseDTO dto)
        {
            var reporter = await _accountRepo.GetAccountById(dto.ReporterId);
            if (reporter != null)
            {
                dto.ReporterFullname = reporter.FullName;
                dto.ReporterPicture = reporter.Picture;
            }


            var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(dto.Id);
            var assignmentDtos = new List<TaskAssignmentResponseDTO>();
            foreach (var a in assignments)
            {
                var assignmentDto = _mapper.Map<TaskAssignmentResponseDTO>(a);
                var account = await _accountRepo.GetAccountById(a.AccountId);
                if (account != null)
                {
                    assignmentDto.AccountFullname = account.FullName;
                    assignmentDto.AccountPicture = account.Picture;
                }
                assignmentDtos.Add(assignmentDto);
            }
            dto.TaskAssignments = assignmentDtos;

            var allComments = await _taskCommentService.GetAllTaskComment();
            var taskComments = allComments.Where(c => c.TaskId == dto.Id).ToList();
            dto.CommentCount = taskComments.Count;
            dto.Comments = _mapper.Map<List<TaskCommentResponseDTO>>(taskComments);

            var labels = await _workItemLabelService.GetByTaskIdAsync(dto.Id);
            var labelDtos = new List<LabelResponseDTO>();
            foreach (var l in labels)
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId);
                labelDtos.Add(_mapper.Map<LabelResponseDTO>(label));
            }
            dto.Labels = labelDtos;
        }

        public async Task<TaskResponseDTO> ChangeTaskTitle(string id, string title, int createdBy)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Title with task ID {id} not found.");

            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");


            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            entity.Title = title;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed title of task '{entity.Id}' to '{title}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task title: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskSprint(string id, int sprintId, int createdBy)
        {
            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");


            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

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
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed sprint of task '{entity.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task sprint: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedStartDate(string id, DateTime plannedStartDate, int createdBy)
        {
            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            // Validate planned_start_date < planned_end_date if end date exists
            //if (entity.PlannedEndDate.HasValue && plannedStartDate > entity.PlannedEndDate.Value)
            //    throw new ArgumentException("Planned start date cannot be after planned end date.");

            entity.PlannedStartDate = plannedStartDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await CalculatePlannedHoursAsync(entity.Id);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed planned start date of task '{entity.Id}' to '{plannedStartDate}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task title: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedEndDate(string id, DateTime plannedEndDate, int createdBy)
        {
            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned EndDate with task ID {id} not found.");

            entity.PlannedEndDate = plannedEndDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await CalculatePlannedHoursAsync(entity.Id);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed Planned End Date of task '{entity.Id}' to '{plannedEndDate}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task Planned EndDate: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskDescription(string id, string description, int createdBy)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.UPDATE.ToString();

            entity.Description = description;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed description of task '{entity.Id}' to '{description}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task Description: {ex.Message}", ex);
            }
            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedHours(string id, decimal plannedHours, int createdBy)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var actualHours = entity.ActualHours ?? 0;
            entity.RemainingHours = plannedHours - actualHours;
            entity.PlannedHours = plannedHours;
            entity.UpdatedAt = DateTime.UtcNow;

            var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(id);
            var assignedAccountIds = taskAssignments.Select(a => a.AccountId).Distinct().ToList();

            var projectMembers = new List<ProjectMember>();

            foreach (var accountId in assignedAccountIds)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, entity.ProjectId);
                if (member != null && member.WorkingHoursPerDay.HasValue)
                {
                    decimal hourlyRate = member.HourlyRate ?? 0m;
                    projectMembers.Add(new ProjectMember
                    {
                        Id = member.Id,
                        AccountId = member.AccountId,
                        ProjectId = member.ProjectId,
                        WorkingHoursPerDay = member.WorkingHoursPerDay,
                        HourlyRate = hourlyRate,
                    });
                }
            }

            decimal totalWorkingHoursPerDay = projectMembers.Sum(m => m.WorkingHoursPerDay.Value);
            decimal totalCost = 0m;

            if (totalWorkingHoursPerDay > 0)
            {
                foreach (var member in projectMembers)
                {
                    //var ratio = member.WorkingHoursPerDay.Value / totalWorkingHoursPerDay;
                    //var memberAssignedHours = plannedHours * ratio;
                    //var memberCost = memberAssignedHours * member.HourlyRate.Value;

                    //var memberAssignedHours = plannedHours * (member.WorkingHoursPerDay ?? 0) / totalWorkingHoursPerDay;
                    //var memberCost = memberAssignedHours * (member.HourlyRate ?? 0);
                    //totalCost += memberCost;
                    var memberAssignedHours = plannedHours * (member.WorkingHoursPerDay.Value / totalWorkingHoursPerDay);
                    var memberCost = memberAssignedHours * member.HourlyRate.Value;
                    totalCost += memberCost;

                    var taskAssignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(id, member.AccountId);
                    if (taskAssignment != null)
                    {
                        taskAssignment.PlannedHours = memberAssignedHours;
                        await _taskAssignmentRepo.Update(taskAssignment);
                    }
                }

                entity.PlannedResourceCost = totalCost;
                //entity.PlannedCost = totalCost;
            }
            else
            {
                // Warning: No assignments or working hours, costs remain 0
                entity.PlannedResourceCost = 0m;
                //entity.PlannedCost = 0m;
            }

            try
            {
                await _taskRepo.Update(entity);
                //await UpdateTaskProgressAsync(entity);

              

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = entity.ProjectId,
                    TaskId = id,
                    SubtaskId = null,
                    RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString(),
                    RelatedEntityId = id,
                    ActionType = ActivityLogActionTypeEnum.UPDATE.ToString(),
                    Message = $"Updated plan hours for task '{id}' to {plannedHours}",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task PlannedHours: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        //public async Task<TaskResponseDTO> ChangeTaskPlannedHours(string id, decimal plannedHours, int createdBy)
        //{
        //    var entity = await _taskRepo.GetByIdAsync(id);
        //    if (entity == null)
        //        throw new KeyNotFoundException($"Task with ID {id} not found.");

        //    var actualHours = entity.ActualHours ?? 0;
        //    entity.RemainingHours = plannedHours - actualHours;
        //    entity.PlannedHours = plannedHours;
        //    entity.UpdatedAt = DateTime.UtcNow;

        //    var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(id);
        //    if (!taskAssignments.Any())
        //    {
        //        Console.WriteLine("No task assignments found for task.");
        //        entity.PlannedResourceCost = 0m;
        //        entity.PlannedCost = 0m;
        //        await _taskRepo.Update(entity);
        //        await UpdateTaskProgressAsync(entity);
        //        return _mapper.Map<TaskResponseDTO>(entity);
        //    }

        //    var assignedAccountIds = taskAssignments.Select(a => a.AccountId).Distinct().ToList();
        //    var projectMembers = new List<ProjectMember>();
        //    Console.WriteLine($"TaskAssignments count: {taskAssignments.Count}");

        //    foreach (var accountId in assignedAccountIds)
        //    {
        //        var member = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, entity.ProjectId);
        //        if (member != null && member.WorkingHoursPerDay.HasValue)
        //        {
        //            decimal hourlyRate = member.HourlyRate ?? 0m;
        //            projectMembers.Add(new ProjectMember
        //            {
        //                Id = member.Id,
        //                AccountId = member.AccountId,
        //                ProjectId = member.ProjectId,
        //                WorkingHoursPerDay = member.WorkingHoursPerDay,
        //                HourlyRate = hourlyRate,
        //            });
        //        }
        //    }

        //    decimal totalWorkingHoursPerDay = projectMembers.Sum(m => m.WorkingHoursPerDay.Value);
        //    Console.WriteLine($"TotalWorkingHoursPerDay: {totalWorkingHoursPerDay}");
        //    decimal totalCost = 0m;

        //    if (totalWorkingHoursPerDay > 0)
        //    {
        //        foreach (var member in projectMembers)
        //        {
        //            var memberAssignedHours = Math.Round(plannedHours * (member.WorkingHoursPerDay.Value / totalWorkingHoursPerDay), 2);
        //            var memberCost = memberAssignedHours * member.HourlyRate.Value;
        //            totalCost += memberCost;
        //            Console.WriteLine($"Calculated memberAssignedHours for AccountId {member.AccountId}: {memberAssignedHours}");

        //            var taskAssignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(id, member.AccountId);
        //            Console.WriteLine($"TaskAssignment for AccountId {member.AccountId}: {(taskAssignment != null ? "Found" : "Not Found")}");
        //            if (taskAssignment != null)
        //            {
        //                taskAssignment.PlannedHours = memberAssignedHours;
        //                await _taskAssignmentRepo.Update(taskAssignment);
        //                Console.WriteLine($"Updated TaskAssignment for AccountId {member.AccountId} with PlannedHours: {taskAssignment.PlannedHours}");

        //                // Kiểm tra lại giá trị sau khi cập nhật
        //                var updatedTaskAssignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(id, member.AccountId);
        //                Console.WriteLine($"Verified PlannedHours for AccountId {member.AccountId}: {updatedTaskAssignment.PlannedHours}");
        //            }
        //        }

        //        entity.PlannedResourceCost = totalCost;
        //        entity.PlannedCost = totalCost;
        //    }
        //    else
        //    {
        //        Console.WriteLine("TotalWorkingHoursPerDay is 0, setting costs to 0.");
        //        entity.PlannedResourceCost = 0m;
        //        entity.PlannedCost = 0m;
        //    }

        //    try
        //    {
        //        await _taskRepo.Update(entity);
        //        await UpdateTaskProgressAsync(entity);

        //        await _activityLogService.LogAsync(new ActivityLog
        //        {
        //            ProjectId = entity.ProjectId,
        //            TaskId = id,
        //            SubtaskId = null,
        //            RelatedEntityType = "Task",
        //            RelatedEntityId = id,
        //            ActionType = "UPDATE",
        //            Message = $"Updated plan hours for task '{id}' to {plannedHours}",
        //            CreatedBy = createdBy,
        //            CreatedAt = DateTime.UtcNow
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to change task PlannedHours: {ex.Message}", ex);
        //    }

        //    return _mapper.Map<TaskResponseDTO>(entity);
        //}

        public async Task<TaskResponseDTO> ChangeTaskEpic(string id, string epicId, int createdBy)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_status");
            var dynamicPriority = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("task_priority");
            //var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "TASK");
            //var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "CREATE");
            var dynamicEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString();
            var dynamicActionType = ActivityLogActionTypeEnum.CREATE.ToString();

            var sprint = await _epicRepo.GetByIdAsync(epicId);
            if (sprint == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            entity.EpicId = epicId;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString(),
                    RelatedEntityId = entity.Id,
                    ActionType = ActivityLogActionTypeEnum.UPDATE.ToString(),
                    Message = $"Changed epic of task '{entity.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                }); 
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task sprint: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskWithSubtaskDTO?> GetTaskWithSubtasksAsync(string id)
        {
            return await _taskRepo.GetTaskWithSubtasksAsync(id);
        }

        public async Task<List<TaskBacklogResponseDTO>> GetBacklogTasksAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");


            var entities = await _taskRepo.GetByProjectIdAsync(project.Id);

            var backlogTasks = entities.Where(t => t.SprintId == null).ToList();

            var dtos = _mapper.Map<List<TaskBacklogResponseDTO>>(backlogTasks);
            await EnrichTaskBacklogResponses(dtos);
            return dtos;
        }


        public async Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdAsync(int sprintId)
        {

            var sprint = await _sprintRepo.GetByIdAsync(sprintId);
            if (sprint == null)
                throw new KeyNotFoundException($"Sprint with ID {sprintId} not found.");

            var entities = await _taskRepo.GetBySprintIdAsync(sprintId);

            var dtos = _mapper.Map<List<TaskBacklogResponseDTO>>(entities);
            await EnrichTaskBacklogResponses(dtos);
            return dtos;
        }

        public async Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdByStatusAsync(int sprintId, string status)
        {

            var sprint = await _sprintRepo.GetByIdAsync(sprintId);
            if (sprint == null)
                throw new KeyNotFoundException($"Sprint with ID {sprintId} not found.");

            var entities = await _taskRepo.GetBySprintIdAndByStatusAsync(sprintId, status);

            var dtos = _mapper.Map<List<TaskBacklogResponseDTO>>(entities);
            await EnrichTaskBacklogResponses(dtos);
            return dtos;
        }

        public async Task<List<TaskBacklogResponseDTO>> GetTasksByAccountIdAsync(int accountId)
        {
            try
            {
                var account = await _accountRepo.GetAccountById(accountId);
                if (account == null)
                    throw new KeyNotFoundException($"Account with ID {accountId} not found.");

                var taskAssignments = await _taskAssignmentRepo.GetTasksByAccountIdAsync(accountId);

                var tasks = taskAssignments
                    .Where(ta => ta.Task != null)
                    .Select(ta => ta.Task)
                    .ToList();

                if (tasks == null || !tasks.Any())
                {
                    throw new KeyNotFoundException($"No tasks found for account with ID {accountId}.");
                }


                var dtos = _mapper.Map<List<TaskBacklogResponseDTO>>(tasks);
                await EnrichTaskBacklogResponses(dtos);

                return dtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve tasks for account {accountId}: {ex.Message}", ex);
            }
        }

        private async Task EnrichTaskBacklogResponses(List<TaskBacklogResponseDTO> dtos)
        {
            foreach (var dto in dtos)
            {
                var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(dto.Id);
                dto.TaskAssignments = _mapper.Map<List<TaskAssignmentResponseDTO>>(assignments);
            }

        }

        public async Task CalculatePlannedHoursAsync(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null || !task.PlannedStartDate.HasValue || !task.PlannedEndDate.HasValue)
                return; // No dates, skip

            var start = task.PlannedStartDate.Value.Date;
            var end = task.PlannedEndDate.Value.Date;

            // Calculate business days (exclude weekends)
            var businessDays = 0;
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    businessDays++;
            }

            // Get assignments and sum working hours per day
            var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);
            decimal totalWorkingHoursPerDay = 0m;
            foreach (var assignment in assignments)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, task.ProjectId);
                totalWorkingHoursPerDay += member?.WorkingHoursPerDay ?? 0m;
            }

            // Estimate planned hours
            task.PlannedHours = businessDays * totalWorkingHoursPerDay;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.Update(task);
        }

        public async Task<TaskResponseDTO> ChangeTaskPercentComlete(string id, decimal? percentComplete, int createdBy)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            if (percentComplete.HasValue && (percentComplete < 0 || percentComplete > 100))
                throw new ArgumentException("Percent complete must be between 0 and 100.");

            var oldPercentComplete = task.PercentComplete;
            task.PercentComplete = percentComplete;
            await _taskRepo.Update(task);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString(),
                RelatedEntityId = task.Id,
                ActionType = ActivityLogActionTypeEnum.UPDATE.ToString(),
                FieldChanged = "PercentComplete",
                OldValue = oldPercentComplete?.ToString() ?? "null",
                NewValue = percentComplete?.ToString() ?? "null",
                Message = $"Updated percent complete for task '{task.Id}' to {percentComplete ?? 0}",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            }); 

            return _mapper.Map<TaskResponseDTO>(task);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedCost(string id, decimal? plannedCost, int createdBy)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            if (plannedCost.HasValue && plannedCost < 0)
                throw new ArgumentException("Planned cost cannot be negative.");

            var oldPlannedCost = task.PlannedCost;
            task.PlannedCost = plannedCost;
            await _taskRepo.Update(task);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString(),
                RelatedEntityId = task.Id,
                ActionType = ActivityLogActionTypeEnum.UPDATE.ToString(),
                FieldChanged = "PlannedCost",
                OldValue = oldPlannedCost?.ToString() ?? "null",
                NewValue = plannedCost?.ToString() ?? "null",
                Message = $"Updated planned cost for task '{task.Id}' to {plannedCost ?? 0}",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<TaskResponseDTO>(task);
        }

        public async Task<TaskResponseDTO> ChangeTaskActualCost(string id, decimal? actualCost, int createdBy)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            if (actualCost.HasValue && actualCost < 0)
                throw new ArgumentException("Actual cost cannot be negative.");

            var oldActualCost = task.ActualCost;
            task.ActualCost = actualCost;
            await _taskRepo.Update(task);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                RelatedEntityType = ActivityLogRelatedEntityTypeEnum.TASK.ToString(),
                RelatedEntityId = task.Id,
                ActionType = ActivityLogActionTypeEnum.UPDATE.ToString(),
                FieldChanged = "ActualCost",
                OldValue = oldActualCost?.ToString() ?? "null",
                NewValue = actualCost?.ToString() ?? "null",
                Message = $"Updated actual cost for task '{task.Id}' to {actualCost ?? 0}",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<TaskResponseDTO>(task);
        }

        //public async Task<TaskResponseDTO> ChangeTaskActualHours(string id, decimal actualHours, int createdBy)
        //{
        //    var entity = await _taskRepo.GetByIdAsync(id);
        //    if (entity == null)
        //        throw new KeyNotFoundException($"Task with ID {id} not found.");

        //    entity.ActualHours = actualHours;

        //    var plannedHours = entity.PlannedHours ?? 0m;
        //    entity.RemainingHours = plannedHours - actualHours;

        //    // Recalculate costs from assignments (aggregate actual costs)
        //    var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(id);
        //    decimal totalActualCost = 0m;

        //    foreach (var assignment in taskAssignments)
        //    {
        //        var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, entity.ProjectId);
        //        decimal memberHourlyRate = member?.HourlyRate ?? 0m;
        //        totalActualCost += (assignment.ActualHours ?? 0m) * memberHourlyRate; // Use assignment actual hours for cost
        //    }

        //    entity.ActualResourceCost = totalActualCost;
        //    entity.ActualCost = totalActualCost;

        //    await _taskRepo.Update(entity);
        //    await UpdateTaskProgressAsync(entity);

        //    await _activityLogService.LogAsync(new ActivityLog
        //    {
        //        ProjectId = entity.ProjectId,
        //        TaskId = id,
        //        RelatedEntityType = "Task",
        //        RelatedEntityId = id,
        //        ActionType = "UPDATE",
        //        Message = $"Updated actual hours for task '{id}' to {actualHours}",
        //        CreatedBy = createdBy,
        //        CreatedAt = DateTime.UtcNow
        //    });

        //    return _mapper.Map<TaskResponseDTO>(entity);
        //}
        //
    }
}
