using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.Task;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.DTOs.TaskDependency.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.TaskCommentServices;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using IntelliPM.Services.WorkLogServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

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

        public TaskService(IMapper mapper, ITaskRepository taskRepo, IEpicRepository epicRepo, IProjectRepository projectRepo, ISubtaskRepository subtaskRepo, IAccountRepository accountRepo, ITaskCommentService taskCommentService, IWorkItemLabelService workItemLabelService, ITaskAssignmentRepository taskAssignmentRepository, ITaskDependencyRepository taskDependencyRepo, IProjectMemberRepository projectMemberRepo, IDynamicCategoryRepository dynamicCategoryRepo, IWorkLogService workLogService, ISprintRepository sprintRepo, IActivityLogService activityLogService)
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
            var dependencies = await _taskDependencyRepo.GetByTaskIdAsync(id);
            dto.Dependencies = dependencies.Select(d => new TaskDependencyResponseDTO
            {
                Id = d.Id,
                TaskId = d.TaskId,
                LinkedFrom = d.LinkedFrom,
                LinkedTo = d.LinkedTo,
                Type = d.Type
            }).ToList();

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

            var entity = _mapper.Map<Tasks>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
            entity.Priority = "MEDIUM";
            entity.Status = "TO_DO";

            try
            {
                await _taskRepo.Add(entity);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "CREATE",
                    Message = $"Created task '{entity.Id}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });

                if (!string.IsNullOrEmpty(entity.EpicId))
                {
                    var epic = await _epicRepo.GetByIdAsync(entity.EpicId);
                    if (epic != null && epic.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase))
                    {
                        epic.Status = "IN_PROGRESS";
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

            if (request.Dependencies != null)
            {
                await _taskDependencyRepo.DeleteByTaskIdAsync(id);

                var newDeps = request.Dependencies.Select(d => new TaskDependency
                {
                    TaskId = id,
                    LinkedFrom = d.LinkedFrom,
                    LinkedTo = d.LinkedTo,
                    Type = d.Type
                }).ToList();

                // Lưu lại
                await _taskDependencyRepo.AddRangeAsync(newDeps);
            }

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

            var isInProgress = status.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase);
            var isDone = status.Equals("DONE", StringComparison.OrdinalIgnoreCase);

            if (isInProgress)
                entity.ActualStartDate = DateTime.UtcNow;
            if (isDone)
                entity.ActualEndDate = DateTime.UtcNow;

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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

            return _mapper.Map<TaskResponseDTO>(entity);
        }


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
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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

        public async Task<TaskResponseDTO> ChangeTaskPlannedStartDate(string id, DateTime plannedStartDate, int createdBy)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            entity.PlannedStartDate = plannedStartDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                //await CalculatePlannedHoursAsync(entity.Id);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned EndDate with task ID {id} not found.");

            entity.PlannedEndDate = plannedEndDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
                //await CalculatePlannedHoursAsync(entity.Id);
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.Id))?.ProjectId ?? 0,
                    TaskId = entity.Id,
                    //SubtaskId = entity.Subtask,
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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
                    RelatedEntityType = "Task",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
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

        public async Task<TaskResponseDTO> ChangeTaskPlannedHours(string id, decimal plannedHours)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            entity.PlannedHours = plannedHours;
            entity.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task PlannedHours: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskSprint(string id, int sprintId)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var sprint = await _sprintRepo.GetByIdAsync(sprintId);
            if (sprint == null)
                throw new KeyNotFoundException($"Sprint with ID {id} not found.");

            entity.SprintId = sprintId;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task sprint: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskEpic(string id, string epicId)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var sprint = await _epicRepo.GetByIdAsync(epicId);
            if (sprint == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            entity.EpicId = epicId;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
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

        private async Task EnrichTaskBacklogResponses(List<TaskBacklogResponseDTO> dtos)
        {
            foreach (var dto in dtos)
            {
                var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(dto.Id);
                dto.TaskAssignments = _mapper.Map<List<TaskAssignmentResponseDTO>>(assignments);
            }

        }

        public async Task<List<TaskBacklogResponseDTO>> GetTasksBySprintIdByStatusAsync(int sprintId, string status)
        {

            var sprint = await _sprintRepo.GetByIdAsync(sprintId);
            if (sprint == null)
                throw new KeyNotFoundException($"Sprint with ID {sprintId} not found.");

            var entities = await _taskRepo.GetBySprintIdAndByStatusAsync(sprintId,status);

            var dtos = _mapper.Map<List<TaskBacklogResponseDTO>>(entities);
            await EnrichTaskBacklogResponses(dtos);
            return dtos;
        }

        //
    }
}



//public async Task<TaskResponseDTO> CalculatePlannedHoursAsync(string id)
//{
//    var task = await _taskRepo.GetByIdAsync(id);
//    if (task == null)
//        throw new KeyNotFoundException($"Task with ID {id} not found.");

//    if (task.PlannedStartDate == null || task.PlannedEndDate == null)
//        throw new InvalidOperationException("Task must have both planned start and end dates.");

//    var totalDays = (task.PlannedEndDate.Value.Date - task.PlannedStartDate.Value.Date).Days + 1;
//    if (totalDays <= 0)
//        throw new InvalidOperationException("Planned end date must be after start date.");

//    const int workingHoursPerDay = 8;

//    // Lấy số người được gán vào task
//    var assignees = await _taskAssignmentRepo.GetByTaskIdAsync(id);
//    var numAssignees = assignees.Count;

//    if (numAssignees == 0)
//        throw new InvalidOperationException("No assignees assigned to this task.");

//    // Tổng giờ công = ngày × giờ/ngày × số người
//    var plannedHours = totalDays * workingHoursPerDay * numAssignees;
//    task.PlannedHours = plannedHours;
//    task.UpdatedAt = DateTime.UtcNow;

//    await _taskRepo.Update(task);
//    await DistributePlannedHoursAsync(id);

//    decimal totalResourceCost = 0;
//    foreach (var assignment in assignees)
//    {
//        if (assignment.PlannedHours == null || assignment.PlannedHours <= 0) continue;

//        var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, task.ProjectId);
//        if (projectMember?.HourlyRate != null)
//        {
//            var memberCost = assignment.PlannedHours.Value * projectMember.HourlyRate.Value;
//            totalResourceCost += memberCost;
//        }
//    }

//    task.PlannedResourceCost = Math.Round(totalResourceCost, 2);

//    //var otherCost = task.OtherPlannedCost ?? 0;
//    var otherCost = 0;
//    task.PlannedCost = Math.Round(task.PlannedResourceCost.Value + otherCost, 2);

//    await _taskRepo.Update(task);

//    return _mapper.Map<TaskResponseDTO>(task);
//}

//public async Task DistributePlannedHoursAsync(string taskId)
//{
//    var task = await _taskRepo.GetByIdAsync(taskId);
//    if (task == null)
//        throw new KeyNotFoundException($"Task with ID {taskId} not found.");

//    if (task.PlannedHours == null || task.PlannedHours <= 0)
//        throw new InvalidOperationException("Planned hours for task must be greater than 0 before distribution.");

//    var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);
//    if (assignments == null || !assignments.Any())
//        return;

//    decimal totalWorkingHours = 0;
//    var memberWorkingHours = new Dictionary<int, decimal>();

//    foreach (var assignment in assignments)
//    {
//        var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, task.ProjectId);
//        if (projectMember == null)
//            continue;

//        // TODO: Replace this when you add "WorkingHoursPerDay" column
//        //var workingHours = 8m;
//        var workingHours = projectMember.WorkingHoursPerDay ?? 8;

//        memberWorkingHours[assignment.AccountId] = workingHours;
//        totalWorkingHours += workingHours;
//    }

//    if (totalWorkingHours == 0) return;

//    foreach (var assignment in assignments)
//    {
//        if (!memberWorkingHours.TryGetValue(assignment.AccountId, out var hours)) continue;

//        var share = task.PlannedHours.Value * (hours / totalWorkingHours);
//        assignment.PlannedHours = Math.Round(share, 2);

//        await _taskAssignmentRepo.Update(assignment);
//    }
//}
