using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.Subtask;
using IntelliPM.Data.Enum.Task;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Services.SubtaskCommentServices;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using IntelliPM.Services.WorkLogServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskServices
{
    public class SubtaskService : ISubtaskService
    {
        private readonly IMapper _mapper;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly ILogger<SubtaskService> _logger;
        private readonly ITaskRepository _taskRepo;
        private readonly IGeminiService _geminiService;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAccountRepository _accountRepo; 
        private readonly ISubtaskCommentService _subtaskCommentService;
        private readonly IWorkItemLabelService _workItemLabelService;
        private readonly IWorkLogService _workLogService;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IActivityLogService _activityLogService;
        private readonly ITaskDependencyRepository _taskDependencyRepo;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly IEmailService _emailService;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;
        private readonly IDecodeTokenHandler _decodeToken;
        public SubtaskService(IMapper mapper, ISubtaskRepository subtaskRepo, ILogger<SubtaskService> logger, ITaskRepository taskRepo, IGeminiService geminiService, IEpicRepository epicRepo, IProjectRepository projectRepo, IAccountRepository accountRepo, ISubtaskCommentService subtaskCommentService, IWorkItemLabelService workItemLabelService, IWorkLogService workLogService, IProjectMemberRepository projectMemberRepo, IActivityLogService activityLogService, ITaskDependencyRepository taskDependencyRepo, IMilestoneRepository milestoneRepo, IEmailService emailService, IDynamicCategoryHelper dynamicCategoryHelper, IDecodeTokenHandler decodeToken)
        {
            _mapper = mapper;
            _subtaskRepo = subtaskRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _geminiService = geminiService;
            _epicRepo = epicRepo;
            _projectRepo = projectRepo;
            _accountRepo = accountRepo;
            _subtaskCommentService = subtaskCommentService;
            _workItemLabelService = workItemLabelService;
            _workLogService = workLogService;
            _projectMemberRepo = projectMemberRepo;
            _activityLogService = activityLogService;
            _taskDependencyRepo = taskDependencyRepo;
            _milestoneRepo = milestoneRepo;
            _emailService = emailService;
            _dynamicCategoryHelper = dynamicCategoryHelper;
            _decodeToken = decodeToken; 
        }

        public async Task<List<Subtask>> GenerateSubtaskPreviewAsync(string token, string taskId)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new ApiException(HttpStatusCode.NotFound, "User not found.");

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");
            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_status");
            var dynamicPriority = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_priority");
            var checklistTitles = await _geminiService.GenerateSubtaskAsync(task.Title);
           
            var checklists = checklistTitles.Select(title => new Subtask
            {
                TaskId = taskId,
                Title = title,
                Priority = dynamicPriority,
                Status = dynamicStatus,
                ManualInput = false,
                GenerationAiInput = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ReporterId = currentAccount.Id,
            }).ToList();
            return checklists;
        }

        public async Task<SubtaskResponseDTO> CreateSubtask(SubtaskRequest2DTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Subtask title is required.", nameof(request.Title));

            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");

            var project = await _projectRepo.GetByIdAsync(task.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found.");

            var projectKey = project.ProjectKey;
            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_status");
            var dynamicPriority = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_priority");
            var entity = _mapper.Map<Subtask>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
            entity.Status = dynamicStatus;
            entity.ManualInput = false;
            entity.GenerationAiInput = true;
            entity.Priority = dynamicPriority;

            try
            {
                await _subtaskRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask due to DB error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task<List<SubtaskResponseDTO>> SaveGeneratedSubtasks(List<SubtaskRequest2DTO> previews)
        {
            if (previews == null || previews.Count == 0)
                throw new ArgumentException("No preview subtasks provided");

            var result = new List<SubtaskResponseDTO>();

            foreach (var request in previews)
            {
                var created = await CreateSubtask(request);
                result.Add(created);
            }

            return result;
        }

        public async Task<SubtaskResponseDTO> Create2Subtask(SubtaskRequest2DTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Subtask title is required.", nameof(request.Title));

            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");

            var project = await _projectRepo.GetByIdAsync(task.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found.");

            var dynamicStatus = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_status");
            var dynamicPriority = await _dynamicCategoryHelper.GetDefaultCategoryNameAsync("subtask_priority");
            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "SUBTASK");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "CREATE");
            var projectKey = project.ProjectKey;

            var entity = _mapper.Map<Subtask>(request);

            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
            entity.Status = dynamicStatus;
            entity.Priority = dynamicPriority;
            entity.AssignedBy = null;
            entity.ManualInput = true;
            entity.GenerationAiInput = false;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _subtaskRepo.Add(entity);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = task.ProjectId,
                    TaskId = task.Id,
                    SubtaskId = entity.Id,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Created subtask '{entity.Id}' under task '{task.Id}'",
                    CreatedBy = request.CreatedBy, 
                    CreatedAt = DateTime.UtcNow
                });

                if (task.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase))
                {
                    task.Status = "IN_PROGRESS";
                    task.UpdatedAt = DateTime.UtcNow;
                    task.ActualEndDate = null;
                    await _taskRepo.Update(task);

                    await _activityLogService.LogAsync(new ActivityLog
                    {
                        ProjectId = task.ProjectId,
                        TaskId = task.Id,
                        RelatedEntityType = "Task",
                        RelatedEntityId = task.Id,
                        ActionType = "UPDATE",
                        Message = $"Task '{task.Id}' status changed from DONE to IN_PROGRESS due to new subtask",
                        CreatedBy = request.CreatedBy, 
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask due to DB error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task DeleteSubtask(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            try
            {
                await _subtaskRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete Subtask: {ex.Message}", ex);
            }
        }

        public async Task<List<SubtaskResponseDTO>> GetAllSubtaskList()
        {
            var entities = await _subtaskRepo.GetAllSubtask();
            return _mapper.Map<List<SubtaskResponseDTO>>(entities);
        }

        public async Task<SubtaskResponseDTO> GetSubtaskById(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            var dto = _mapper.Map<SubtaskResponseDTO>(entity);

            var isInProgress = entity.Status.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase);
            var isDone = entity.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase);

            // Bổ sung check trước khi cập nhật trạng thái
            var dependencies = await _taskDependencyRepo
                .FindAllAsync(d => d.LinkedTo == id && d.ToType == "Subtask");

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

                if (Enum.TryParse<TaskDependencyEnum>(dep.Type, true, out var dependencyType))
                {
                    switch (dependencyType)
                    {
                        case TaskDependencyEnum.FINISH_START:
                            // Subtask just can start (IN_PROGRESS or DONE) if LinkedFrom has done (DONE)
                            if ((isInProgress || isDone) && !string.Equals(sourceStatus, SubtaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before starting.");
                            }
                            break;
                        case TaskDependencyEnum.START_START:
                            // Subtask just can start (IN_PROGRESS or DONE) if LinkedFrom has started (IN_PROGRESS or DONE)
                            if ((isInProgress || isDone) && string.Equals(sourceStatus, SubtaskStatusEnum.TO_DO.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be started (IN_PROGRESS or DONE) before starting.");
                            }
                            break;
                        case TaskDependencyEnum.FINISH_FINISH:
                            // Subtask has just been done (DONE) if LinkedFrom has done (DONE)
                            if (isDone && !string.Equals(sourceStatus, SubtaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed (DONE) before it can be completed.");
                            }
                            break;
                        case TaskDependencyEnum.START_FINISH:
                            // Subtask has just been done (DONE) if LinkedFrom has started (IN_PROGRESS or DONE)
                            if (isDone && string.Equals(sourceStatus, SubtaskStatusEnum.TO_DO.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' can only be completed after {dep.FromType.ToLower()} '{dep.LinkedFrom}' has started (IN_PROGRESS or DONE).");
                            }
                            break;
                    }
                }
                else
                {
                    warnings.Add($"Invalid dependency type '{dep.Type}' for subtask '{id}'.");
                }
            }

            dto.Warnings = warnings;
            return dto;
        }

        public async Task<List<SubtaskResponseDTO>> GetSubtaskByTaskIdAsync(string taskId)
        {
            { 
                var entities = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No subtasks found for Task ID {taskId}.");

                return _mapper.Map<List<SubtaskResponseDTO>>(entities);
            }
        }

        public async Task<SubtaskResponseDTO> UpdateSubtask(string id, SubtaskRequestDTO request)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            var oldAssignedBy = entity.AssignedBy; // lưu lại giá trị cũ

            _mapper.Map(request, entity);

            if (request.AssignedBy == 0)
                entity.AssignedBy = null;

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "SUBTASK");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "UPDATE");

            try
            {
                await _subtaskRepo.Update(entity);
                await UpdateSubtaskResourceCosts(entity);

                if (oldAssignedBy != entity.AssignedBy)
                {
                    
                    var assignee = await _accountRepo.GetAccountById(entity.AssignedBy ?? 0);
                    if (assignee != null)
                    {
                        await _emailService.SendSubtaskAssignmentEmail(
                            assignee.FullName,
                            assignee.Email,
                            entity.Id,
                            entity.Title
                        );
                    }
                }

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.TaskId))?.ProjectId ?? 0,
                    TaskId = entity.TaskId,
                    SubtaskId = entity.Id,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Updated subtask '{entity.Id}' under task '{entity.TaskId}'",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update Subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        private async Task UpdateSubtaskResourceCosts(Subtask entity)
        {
            if (entity.AssignedBy == null)
            {
                entity.PlannedResourceCost = 0;
                //entity.PlannedCost = 0;
                entity.ActualResourceCost = 0;
                //entity.ActualCost = 0;
            }
            else
            {
                // Lấy Task để lấy ProjectId
                var task = await _taskRepo.GetByIdAsync(entity.TaskId);
                if (task == null)
                {
                    throw new KeyNotFoundException($"Task with ID {entity.TaskId} not found.");
                }

                // Lấy hourlyRate từ ProjectMember
                var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(entity.AssignedBy.Value, task.ProjectId);
                decimal hourlyRate = projectMember?.HourlyRate ?? 0; 

                // Tính toán chi phí cho Subtask
                entity.PlannedResourceCost = entity.PlannedHours * hourlyRate;
                //entity.PlannedCost = entity.PlannedHours * hourlyRate;
                entity.ActualResourceCost = entity.ActualHours * hourlyRate;
                //entity.ActualCost = entity.ActualHours * hourlyRate;
            }

            // Cập nhật Subtask (chỉ cập nhật chi phí, không cần update toàn bộ nếu không muốn)
            await _subtaskRepo.Update(entity);

            // Cập nhật chi phí cho Task cha bằng tổng từ tất cả Subtasks
            await UpdateTaskResourceCosts(entity.TaskId);
        }

        private async Task UpdateTaskResourceCosts(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");
            }

            // Lấy tất cả Subtasks của Task
            var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

            // Tính tổng chi phí
            task.PlannedResourceCost = subtasks.Sum(s => s.PlannedResourceCost);
            //task.PlannedCost = subtasks.Sum(s => s.PlannedCost);
            task.ActualResourceCost = subtasks.Sum(s => s.ActualResourceCost);
            //task.ActualCost = subtasks.Sum(s => s.ActualCost);

            // Cập nhật Task
            await _taskRepo.Update(task);
        }

        public async Task<SubtaskResponseDTO> ChangeSubtaskStatus(string id, string status, int createdBy)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var validStatuses = await _dynamicCategoryHelper.GetSubtaskStatusesAsync();
            if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid status: '{status}'. Must be one of: {string.Join(", ", validStatuses)}");

            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            var isInProgress = status.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase);
            var isDone = status.Equals("DONE", StringComparison.OrdinalIgnoreCase);

            if (isInProgress)
                entity.ActualStartDate = DateTime.UtcNow;
            if (isDone)
                entity.ActualEndDate = DateTime.UtcNow;

            var dependencies = await _taskDependencyRepo
                .FindAllAsync(d => d.LinkedTo == id && d.ToType == "Subtask");

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

                if (Enum.TryParse<TaskDependencyEnum>(dep.Type, true, out var dependencyType))
                {
                    switch (dependencyType)
                    {
                        case TaskDependencyEnum.FINISH_START:
                            // Subtask just can start (IN_PROGRESS or DONE) if LinkedFrom has done (DONE)
                            if ((isInProgress || isDone) && !string.Equals(sourceStatus, SubtaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before starting.");
                            }
                            break;
                        case TaskDependencyEnum.START_START:
                            // Subtask just can start (IN_PROGRESS or DONE) if LinkedFrom has started (IN_PROGRESS or DONE)
                            if ((isInProgress || isDone) && string.Equals(sourceStatus, SubtaskStatusEnum.TO_DO.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be started (IN_PROGRESS or DONE) before starting.");
                            }
                            break;
                        case TaskDependencyEnum.FINISH_FINISH:
                            // Subtask has just been done (DONE) if LinkedFrom has done (DONE)
                            if (isDone && !string.Equals(sourceStatus, SubtaskStatusEnum.DONE.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed (DONE) before it can be completed.");
                            }
                            break;
                        case TaskDependencyEnum.START_FINISH:
                            // Subtask has just been done (DONE) if LinkedFrom has started (IN_PROGRESS or DONE)
                            if (isDone && string.Equals(sourceStatus, SubtaskStatusEnum.TO_DO.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                warnings.Add($"Subtask '{id}' can only be completed after {dep.FromType.ToLower()} '{dep.LinkedFrom}' has started (IN_PROGRESS or DONE).");
                            }
                            break;
                    }
                }
                else
                {
                    warnings.Add($"Invalid dependency type '{dep.Type}' for subtask '{id}'.");
                }
            }

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            //await UpdateSubtaskProgressAsync(entity);
            //await UpdateTaskProgressBySubtasksAsync(entity.TaskId);

            var dynamicEntityType = await _dynamicCategoryHelper.GetCategoryNameAsync("related_entity_type", "SUBTASK");
            var dynamicActionType = await _dynamicCategoryHelper.GetCategoryNameAsync("action_type", "STATUS_CHANGE");

            try
            {
                await _subtaskRepo.Update(entity);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = (await _taskRepo.GetByIdAsync(entity.TaskId))?.ProjectId ?? 0,
                    TaskId = entity.TaskId,
                    SubtaskId = entity.Id,
                    RelatedEntityType = dynamicEntityType,
                    RelatedEntityId = entity.Id,
                    ActionType = dynamicActionType,
                    Message = $"Changed status of subtask '{entity.Id}' to '{status}' under task '{entity.TaskId}",
                    CreatedBy = createdBy, 
                    CreatedAt = DateTime.UtcNow
                });

                if (isInProgress)
                {
                    await _workLogService.GenerateDailyWorkLogsAsync();
                }

                var parentTaskId = entity.TaskId;
                var allSubtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(parentTaskId);

                var parentTask = await _taskRepo.GetByIdAsync(parentTaskId);
                if (parentTask != null)
                {
                    if (allSubtasks.All(st => st.Status.Equals("DONE", StringComparison.OrdinalIgnoreCase)))
                    {
                        parentTask.Status = "DONE";
                        parentTask.ActualEndDate = DateTime.UtcNow;
                    }
                    else if (allSubtasks.Any(st => st.Status.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase)))
                    {
                        parentTask.Status = "IN_PROGRESS";
                        if (parentTask.ActualStartDate == null)
                            parentTask.ActualStartDate = DateTime.UtcNow;
                    }
                    else
                    {
                        
                    }
                    parentTask.UpdatedAt = DateTime.UtcNow;
                    await _taskRepo.Update(parentTask);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change subtask status: {ex.Message}", ex);
            }

            // return _mapper.Map<SubtaskResponseDTO>(entity);
            var result = _mapper.Map<SubtaskResponseDTO>(entity);
            result.Warnings = warnings;
            return result;
        }

        //private async Task UpdateSubtaskProgressAsync(Subtask subtask)
        //{
        //    if (subtask.Status == "DONE") subtask.PercentComplete = 100;
        //    else if (subtask.Status == "TO_DO") subtask.PercentComplete = 0;
        //    else if (subtask.Status == "IN_PROGRESS")
        //    {
        //        var actual = subtask.ActualHours ?? 0;
        //        var planned = subtask.PlannedHours ?? 1; 
        //        var rawProgress = (actual / planned) * 100;
        //        subtask.PercentComplete = Math.Min((int)rawProgress, 99);
        //    }

        //    subtask.UpdatedAt = DateTime.UtcNow;
        //    await _subtaskRepo.Update(subtask);
        //}

        //private async Task UpdateTaskProgressBySubtasksAsync(string taskId)
        //{
        //    var task = await _taskRepo.GetByIdAsync(taskId);
        //    if (task == null) return;

        //    var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

        //    if (!subtasks.Any())
        //    {
        //        task.PercentComplete = 0;
        //    }
        //    else
        //    {
        //        var avg = subtasks.Average(st => st.PercentComplete ?? 0); 
        //        task.PercentComplete = (int)Math.Round(avg);
        //    }

        //    task.UpdatedAt = DateTime.UtcNow;
        //    await _taskRepo.Update(task);
        //}


        public async Task<SubtaskDetailedResponseDTO> GetSubtaskByIdDetailed(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            var dto = _mapper.Map<SubtaskDetailedResponseDTO>(entity);
            await EnrichSubtaskDetailedResponse(dto);
            return dto;
        }
        public async Task<List<SubtaskDetailedResponseDTO>> GetSubtaskByTaskIdDetailed(string taskId)
        {
            var entities = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);
            var dtos = _mapper.Map<List<SubtaskDetailedResponseDTO>>(entities); 
            foreach (var dto in dtos)
            {
                await EnrichSubtaskDetailedResponse(dto);
            }
            return dtos;
        }

        public async Task<List<SubtaskDetailedResponseDTO>> GetSubtasksByProjectIdDetailed(int projectId)
        {
        
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            if (!tasks.Any())
            {
                return new List<SubtaskDetailedResponseDTO>();
            }

            var taskIds = tasks.Select(t => t.Id).ToList();
            var allSubtasks = new List<SubtaskDetailedResponseDTO>();

       
            foreach (var taskId in taskIds)
            {
                var subtasks = await GetSubtaskByTaskIdDetailed(taskId);
                allSubtasks.AddRange(subtasks);
            }

            return allSubtasks.OrderBy(s => s.CreatedAt).ToList();
        }

        private async Task EnrichSubtaskDetailedResponse(SubtaskDetailedResponseDTO dto)
        {
            if (dto.ReporterId.HasValue)
            {
                var reporter = await _accountRepo.GetAccountById(dto.ReporterId.Value);
                if (reporter != null)
                {
                    dto.ReporterFullname = reporter.FullName;
                    dto.ReporterPicture = reporter.Picture;
                }
            }

            if (dto.AssignedBy.HasValue)
            {
                var assignedBy = await _accountRepo.GetAccountById(dto.AssignedBy.Value);
                if (assignedBy != null)
                {
                    dto.AssignedByFullname = assignedBy.FullName;
                    dto.AssignedByPicture = assignedBy.Picture;
                }
            }

            var allComments = await _subtaskCommentService.GetAllSubtaskComment();
            var subtaskComments = allComments.Where(c => c.SubtaskId == dto.Id).ToList();
            dto.CommentCount = subtaskComments.Count;
            dto.Comments = _mapper.Map<List<SubtaskCommentResponseDTO>>(subtaskComments);


            var labels = await _workItemLabelService.GetBySubtaskIdAsync(dto.Id);
            var labelDtos = new List<LabelResponseDTO>();
            foreach (var l in labels)
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId);
                labelDtos.Add(_mapper.Map<LabelResponseDTO>(label));
            }
            dto.Labels = labelDtos;
        }

        public async Task<SubtaskFullResponseDTO> ChangePlannedHours(string id, decimal hours, int createdBy)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            entity.PlannedHours = hours;

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            if (task != null && entity.AssignedBy != null)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(entity.AssignedBy.Value, task.ProjectId);
                if (member != null && member.HourlyRate.HasValue)
                {
                    decimal plannedResourceCost = hours * (member.HourlyRate ?? 0);
                    entity.PlannedResourceCost = plannedResourceCost;
                    //entity.PlannedCost = plannedResourceCost;
                }
            }

            await _subtaskRepo.Update(entity);
            //await UpdateSubtaskProgressAsync(entity);
            //await UpdateTaskProgressBySubtasksAsync(entity.TaskId);

            try
            {
                var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(entity.TaskId);
                var totalPlannedHours = subtasks.Sum(s => s.PlannedHours ?? 0);
                var totalPlannedResourceCost = subtasks.Sum(s => s.PlannedResourceCost ?? 0);

                if (task != null)
                {
                    var actualHours = task.ActualHours ?? 0;
                    task.RemainingHours = totalPlannedHours - actualHours;
                    task.PlannedHours = totalPlannedHours;
                    task.PlannedResourceCost = totalPlannedResourceCost;
                    //task.PlannedCost = totalPlannedResourceCost;
                    await _taskRepo.Update(task);
                }

                // Ghi log hoạt động
                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = task?.ProjectId,
                    TaskId = task?.Id,
                    SubtaskId = entity.Id,
                    RelatedEntityType = "Subtask",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
                    Message = $"Updated planned hours for subtask '{entity.Id}' to {hours} under task '{task?.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task values: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }

        public async Task<SubtaskFullResponseDTO> ChangeActualHours(string id, decimal hours, int createdBy)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            entity.ActualHours = hours;

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            if (task != null && entity.AssignedBy != null)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(entity.AssignedBy.Value, task.ProjectId);
                if (member != null && member.HourlyRate.HasValue)
                {
                    decimal actualResourceCost = hours * (member.HourlyRate ?? 0);
                    entity.ActualResourceCost = actualResourceCost;
                    //entity.ActualCost = actualResourceCost;
                }
            }

            await _subtaskRepo.Update(entity);
            //await UpdateSubtaskProgressAsync(entity);
            //await UpdateTaskProgressBySubtasksAsync(entity.TaskId);

            try
            {
                var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(entity.TaskId);
                var totalActualHours = subtasks.Sum(s => s.ActualHours ?? 0);
                var totalActualResourceCost = subtasks.Sum(s => s.ActualResourceCost ?? 0);

                if (task != null)
                {
                    var plannedHours = task.PlannedHours ?? 0;
                    task.RemainingHours = plannedHours - totalActualHours;
                    task.ActualHours = totalActualHours;
                    task.ActualResourceCost = totalActualResourceCost;
                    //task.ActualCost = totalActualResourceCost;
                    await _taskRepo.Update(task);
                }

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = task?.ProjectId,
                    TaskId = task?.Id,
                    SubtaskId = entity.Id,
                    RelatedEntityType = "Subtask",
                    RelatedEntityId = entity.Id,
                    ActionType = "UPDATE",
                    Message = $"Updated actual hours for subtask '{entity.Id}' to {hours} under task '{task?.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task values: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }


        public async Task<List<SubtaskResponseDTO>> GetSubTaskByAccountId(int accountId)
        {

            var account = _accountRepo.GetAccountById(accountId);
            if (account != null) throw new KeyNotFoundException($"Account with key {accountId} not found.");

            var entity = await _subtaskRepo.GetByAccountIdAsync(accountId);
           
            return _mapper.Map<List<SubtaskResponseDTO>>(entity);
        }
        public async Task<SubtaskFullResponseDTO> GetFullSubtaskById(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }

        public async Task<SubtaskFullResponseDTO> ChangePercentComplete(string id, decimal? percentComplete, int createdBy)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            if (percentComplete.HasValue && (percentComplete < 0 || percentComplete > 100))
                throw new ArgumentException("Percent complete must be between 0 and 100.");

            entity.PercentComplete = percentComplete;
            await _subtaskRepo.Update(entity);

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(entity.TaskId);
            var totalPercentComplete = subtasks.Sum(s => s.PercentComplete ?? 0);

            if (task != null)
            {
                task.PercentComplete = totalPercentComplete;
                await _taskRepo.Update(task);
            }

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task?.ProjectId,
                TaskId = task?.Id,
                SubtaskId = entity.Id,
                RelatedEntityType = "Subtask",
                RelatedEntityId = entity.Id,
                ActionType = "UPDATE",
                FieldChanged = "PercentComplete",
                OldValue = entity.PercentComplete?.ToString() ?? "null",
                NewValue = percentComplete?.ToString() ?? "null",
                Message = $"Updated percent complete for subtask '{entity.Id}' to {percentComplete ?? 0} under task '{task?.Id}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }

        public async Task<SubtaskFullResponseDTO> ChangePlannedCost(string id, decimal? plannedCost, int createdBy)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            if (plannedCost.HasValue && plannedCost < 0)
                throw new ArgumentException("Planned cost cannot be negative.");

            var oldValue = entity.PlannedCost;
            entity.PlannedCost = plannedCost;
            await _subtaskRepo.Update(entity);

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            if (task != null)
            {
                // Optional: Cập nhật task nếu cần (ví dụ: tổng PlannedCost của các subtask)
                var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(entity.TaskId);
                task.PlannedCost = subtasks.Sum(s => s.PlannedCost ?? 0);
                await _taskRepo.Update(task);
            }

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task?.ProjectId,
                TaskId = task?.Id,
                SubtaskId = entity.Id,
                RelatedEntityType = "Subtask",
                RelatedEntityId = entity.Id,
                ActionType = "UPDATE",
                FieldChanged = "PlannedCost",
                OldValue = oldValue?.ToString() ?? "null",
                NewValue = plannedCost?.ToString() ?? "null",
                Message = $"Updated planned cost for subtask '{entity.Id}' to {plannedCost ?? 0} under task '{task?.Id}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }

        public async Task<SubtaskFullResponseDTO> ChangeActualCost(string id, decimal? actualCost, int createdBy)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            if (actualCost.HasValue && actualCost < 0)
                throw new ArgumentException("Actual cost cannot be negative.");

            var oldValue = entity.ActualCost;
            entity.ActualCost = actualCost;
            await _subtaskRepo.Update(entity);

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            if (task != null)
            {
                // Optional: Cập nhật task nếu cần (ví dụ: tổng ActualCost của các subtask)
                var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(entity.TaskId);
                task.ActualCost = subtasks.Sum(s => s.ActualCost ?? 0);
                await _taskRepo.Update(task);
            }

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task?.ProjectId,
                TaskId = task?.Id,
                SubtaskId = entity.Id,
                RelatedEntityType = "Subtask",
                RelatedEntityId = entity.Id,
                ActionType = "UPDATE",
                FieldChanged = "ActualCost",
                OldValue = oldValue?.ToString() ?? "null",
                NewValue = actualCost?.ToString() ?? "null",
                Message = $"Updated actual cost for subtask '{entity.Id}' to {actualCost ?? 0} under task '{task?.Id}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<SubtaskFullResponseDTO>(entity);
        }
    }
}


