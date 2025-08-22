using AutoMapper;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.EmailServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliPM.Services.ActivityLogServices;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IntelliPM.Services.TaskAssignmentServices
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly ITaskAssignmentRepository _repo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<TaskAssignmentService> _logger;
        private readonly IActivityLogService _activityLogService;


        public TaskAssignmentService(IMapper mapper, ITaskAssignmentRepository repo, ILogger<TaskAssignmentService> logger, IAccountRepository accountRepo, ITaskRepository taskRepo, IEmailService emailService, IProjectMemberRepository projectMemberRepo, IActivityLogService activityLogService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectMemberRepo = projectMemberRepo;
            _accountRepo = accountRepo;
            _taskRepo = taskRepo;
            _emailService = emailService;
            _activityLogService = activityLogService;
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetByTaskIdAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.");

            var entities = await _repo.GetByTaskIdAsync(taskId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No task assignments found for Task ID {taskId}.");

            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetByAccountIdAsync(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0.");

            var entities = await _repo.GetByAccountIdAsync(accountId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No task assignments found for Account ID {accountId}.");

            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<TaskAssignmentResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task<TaskAssignmentResponseDTO> CreateTaskAssignment(TaskAssignmentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.TaskId))
                throw new ArgumentException("Task ID is required.", nameof(request.TaskId));

            var entity = _mapper.Map<TaskAssignment>(request);
            entity.AssignedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task assignment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }


        public async Task<TaskAssignmentResponseDTO> CreateTaskAssignmentQuick(string taskId, TaskAssignmentQuickRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var entity = _mapper.Map<TaskAssignment>(request);
            entity.TaskId = taskId;
            entity.AssignedAt = DateTime.UtcNow;
            entity.Status = "ASSIGNED";

            try
            {
                await _repo.Add(entity);
                var assignedUser = await _accountRepo.GetAccountById(entity.AccountId);
                var task = await _taskRepo.GetByIdAsync(taskId);

                if (assignedUser != null && task != null)
                {
                    await _emailService.SendTaskAssignmentEmail(
                        assigneeFullName: assignedUser.FullName,
                        assigneeEmail: assignedUser.Email,
                        taskId: taskId,
                        taskTitle: task.Title
                    );
                }
                // Recalculate task planned hours after adding the new assignment
                await RecalculateTaskPlannedHours(taskId);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task assignment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        private async Task RecalculateTaskPlannedHours(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return;

            var taskAssignments = await _repo.GetByTaskIdAsync(taskId);
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

                    var taskAssignment = await _repo.GetByTaskAndAccountAsync(taskId, member.AccountId);
                    if (taskAssignment != null)
                    {
                        taskAssignment.PlannedHours = memberAssignedHours;
                        await _repo.Update(taskAssignment);
                    }
                }

                task.PlannedResourceCost = totalCost;
                task.PlannedCost = totalCost;
            }
            else
            {
                // Warning: No assignments or working hours, costs remain 0
                task.PlannedResourceCost = 0m;
                task.PlannedCost = 0m;
            }

            await _taskRepo.Update(task);
            //await UpdateTaskProgressAsync(task);
        }


        public async Task<TaskAssignmentResponseDTO> UpdateTaskAssignment(int id, TaskAssignmentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.AssignedAt = entity.AssignedAt ?? DateTime.UtcNow; 

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task DeleteTaskAssignment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task assignment: {ex.Message}", ex);
            }
        }

        public async Task<TaskAssignmentResponseDTO> ChangeStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            entity.Status = status;
            if (status.ToLower() == "completed")
                entity.CompletedAt = DateTime.UtcNow;
            else
                entity.CompletedAt = null;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task assignment status: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task<List<TaskAssignmentResponseDTO>> CreateListTaskAssignment(List<TaskAssignmentRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("List of task assignments cannot be null or empty.");

            var responses = new List<TaskAssignmentResponseDTO>();
            foreach (var request in requests)
            {
                var response = await CreateTaskAssignment(request);
                responses.Add(response);
            }
            return responses;
        }

        public async Task DeleteByTaskAndAccount(string taskId, int accountId)
        {
            var entities = await _repo.GetByTaskIdAndAccountIdAsync(taskId, accountId);
            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No task assignment found with taskId={taskId} and accountId={accountId}.");

            try
            {
                foreach (var entity in entities)
                {
                    await _repo.Delete(entity);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task assignment: {ex.Message}", ex);
            }
        }

        //public async Task<List<TaskAssignmentHourDTO>> GetTaskAssignmentHoursByTaskIdAsync(string taskId)
        //{
        //    if (string.IsNullOrEmpty(taskId))
        //        throw new ArgumentException("Task ID cannot be null or empty.");

        //    var entities = await _repo.GetByTaskIdAsync(taskId);
        //    if (!entities.Any())
        //        throw new KeyNotFoundException($"No task assignments found for Task ID {taskId}.");

        //    return _mapper.Map<List<TaskAssignmentHourDTO>>(entities);
        //}

        public async Task<List<TaskAssignmentHourDTO>> GetTaskAssignmentHoursByTaskIdAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.");

            // 1. Lấy task để biết projectId
            var taskEntity = await _taskRepo.GetByIdAsync(taskId);
            if (taskEntity == null)
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");

            var projectId = taskEntity.ProjectId;

            // 2. Lấy các assignment của task
            var assignments = await _repo.GetByTaskIdAsync(taskId);
            if (!assignments.Any())
                throw new KeyNotFoundException($"No task assignments found for Task ID {taskId}.");

            // 3. Lấy accountId của các assignment
            var accountIds = assignments.Select(a => a.AccountId).Distinct().ToList();

            // 4. Lấy thông tin ProjectMember tương ứng
            var projectMembers = await _projectMemberRepo.GetByProjectIdAndAccountIdsAsync(projectId, accountIds);

            // 5. Map sang DTO và thêm HourlyRate + WorkingHoursPerDay
            var result = assignments.Select(a =>
            {
                var dto = _mapper.Map<TaskAssignmentHourDTO>(a);
                var pm = projectMembers.FirstOrDefault(m => m.AccountId == a.AccountId);
                if (pm != null)
                {
                    dto.HourlyRate = pm.HourlyRate;
                    dto.WorkingHoursPerDay = pm.WorkingHoursPerDay;
                }
                return dto;
            }).ToList();

            return result;
        }

        public async Task<TaskAssignmentHourDTO> ChangeActualHours(int id, decimal hours)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            entity.ActualHours = hours;
            await _repo.Update(entity);

            // Lấy tất cả TaskAssignment cùng TaskId
            var assignments = await _repo.GetByTaskIdAsync(entity.TaskId);

            // Cập nhật Task.ActualHours = tổng ActualHours của các assignment
            var totalActualHours = assignments.Sum(x => x.ActualHours ?? 0);

            var task = await _taskRepo.GetByIdAsync(entity.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {entity.TaskId} not found.");

            task.ActualHours = totalActualHours;

            // Tính ActualResourceCost
            decimal totalResourceCost = 0m;
            foreach (var assign in assignments)
            {
                var actualHours = assign.ActualHours ?? 0;
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assign.AccountId, task.ProjectId);
                var rate = member?.HourlyRate ?? 0;

                totalResourceCost += actualHours * rate;
            }

            task.ActualResourceCost = totalResourceCost;

            await _taskRepo.Update(task);
            //await _activityLogService.LogAsync(new ActivityLog
            //{
            //    ProjectId = task.ProjectId,
            //    TaskId = task.Id,
            //    RelatedEntityType = "Task",
            //    RelatedEntityId = task.Id,
            //    ActionType = "UPDATE",
            //    Message = $"Updated actual hours for task '{id}' to {hours}",
            //    CreatedBy = createdBy,
            //    CreatedAt = DateTime.UtcNow
            //});

            return new TaskAssignmentHourDTO
            {
                Id = entity.Id,
                TaskId = entity.TaskId,
                AccountId = entity.AccountId,
                ActualHours = entity.ActualHours
            };
        }

        public async Task<bool> ChangeActualHoursAsync(List<TaskAssignmentHourRequestDTO> updates, int createdBy)
        {
            var taskAssignmentsToUpdate = new List<TaskAssignment>();
            var taskIdMap = new Dictionary<string, List<TaskAssignment>>();

            foreach (var update in updates)
            {
                var assignment = await _repo.GetByIdAsync(update.Id);
                if (assignment == null)
                    continue;

                assignment.ActualHours = update.ActualHours;
                taskAssignmentsToUpdate.Add(assignment);

                if (!taskIdMap.ContainsKey(assignment.TaskId))
                    taskIdMap[assignment.TaskId] = new List<TaskAssignment>();

                taskIdMap[assignment.TaskId].Add(assignment);
            }

            // Cập nhật các assignment
            foreach (var assignment in taskAssignmentsToUpdate)
            {
                await _repo.Update(assignment);
            }

            // Cập nhật các Task liên quan
            foreach (var kvp in taskIdMap)
            {
                var taskId = kvp.Key;

                // Lấy toàn bộ assignment của task này
                var allAssignments = await _repo.GetByTaskIdAsync(taskId);
                var task = await _taskRepo.GetByIdAsync(taskId);
                if (task == null) continue;

                // Tổng actual hours
                task.ActualHours = allAssignments.Sum(x => x.ActualHours ?? 0m);

                // Tính actual cost
                decimal totalCost = 0m;
                foreach (var assign in allAssignments)
                {
                    var hours = assign.ActualHours ?? 0;
                    var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assign.AccountId, task.ProjectId);
                    var rate = member?.HourlyRate ?? 0;

                    totalCost += hours * rate;
                }

                task.ActualResourceCost = totalCost;
                task.ActualCost = totalCost;
                //await UpdateTaskProgressAsync(task);
                await _taskRepo.Update(task);

                await _activityLogService.LogAsync(new ActivityLog
                {
                    ProjectId = task.ProjectId,
                    TaskId = task.Id,
                    SubtaskId = null,
                    RelatedEntityType = "Task",
                    RelatedEntityId = task.Id,
                    ActionType = "UPDATE",
                    Message = $"Updated actual hours for task '{task.Id}'",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }
            return true;
        }

        //private async Task UpdateTaskProgressAsync(Tasks task)
        //{
        //    if (task.Status == "DONE")
        //    {
        //        task.PercentComplete = 100;
        //    }
        //    else if (task.Status == "TO_DO")
        //    {
        //        task.PercentComplete = 0;
        //    }
        //    else if (task.Status == "IN_PROGRESS")
        //    {
        //        if (task.PlannedHours > 0)
        //        {
        //            var rawProgress = (task.ActualHours / task.PlannedHours) * 100;
        //            task.PercentComplete = Math.Min((int)rawProgress, 99);
        //        }
        //        else
        //        {
        //            task.PercentComplete = 0;
        //        }
        //    }

        //    task.UpdatedAt = DateTime.UtcNow;
        //    await _taskRepo.Update(task);
        //}

        //public async Task UpdateTaskAssignmentPlannedHoursAsync(string taskId, decimal plannedHours, int createdBy)
        //{
        //    // Fetch the task
        //    var task = await _taskRepo.GetByIdAsync(taskId);
        //    if (task == null)
        //        throw new KeyNotFoundException($"Task with ID {taskId} not found.");

        //    // Fetch all task assignments for the task
        //    var taskAssignments = await _repo.GetByTaskIdAsync(taskId)
        //        .Include(ta => ta.Account)
        //        .ToListAsync();

        //    if (!taskAssignments.Any())
        //        throw new InvalidOperationException($"No assignments found for task with ID {taskId}.");

        //    // Fetch project members to get workingHoursPerDay
        //    var assignedAccountIds = taskAssignments.Select(ta => ta.AccountId).Distinct().ToList();
        //    var projectMembers = await _projectMemberRepo.GetByAccountAndProjectAsync(assignedAccountIds, task.ProjectId)
        //        .ToListAsync();

        //    if (!projectMembers.Any())
        //        throw new InvalidOperationException($"No project members found for task with ID {taskId}.");

        //    // Create a mapping of accountId to workingHoursPerDay
        //    var memberMaxHours = projectMembers.ToDictionary(
        //        m => m.AccountId,
        //        m => m.WorkingHoursPerDay ?? 8m // Default to 8 if null
        //    );

        //    // Sort assignments by accountId for consistent distribution
        //    var assignmentsWithMaxHours = taskAssignments
        //        .Select(ta => new
        //        {
        //            TaskAssignment = ta,
        //            MaxHours = memberMaxHours.ContainsKey(ta.AccountId) ? memberMaxHours[ta.AccountId] : 8m
        //        })
        //        .OrderBy(a => a.TaskAssignment.AccountId)
        //        .ToList();

        //    decimal totalMaxHours = assignmentsWithMaxHours.Sum(a => a.MaxHours);
        //    var assignedHours = new List<decimal>();

        //    if (totalMaxHours <= 0)
        //    {
        //        // If no valid working hours, set all to 0 and warn
        //        foreach (var assignment in assignmentsWithMaxHours)
        //        {
        //            assignment.TaskAssignment.PlannedHours = 0m;
        //            await _repo.Update(assignment.TaskAssignment);
        //        }
        //        task.PlannedResourceCost = 0m;
        //        task.PlannedCost = 0m;
        //    }
        //    else
        //    {
        //        // Distribute plannedHours
        //        decimal remainingHours = plannedHours;
        //        foreach (var assignment in assignmentsWithMaxHours)
        //        {
        //            decimal assigned = Math.Min(assignment.MaxHours, remainingHours);
        //            assignment.TaskAssignment.PlannedHours = assigned;
        //            assignedHours.Add(assigned);
        //            remainingHours -= assigned;

        //            if (remainingHours <= 0) break;

        //            await _repo.Update(assignment.TaskAssignment);
        //        }

        //        // If there are remaining hours and more assignments, distribute proportionally
        //        if (remainingHours > 0 && assignmentsWithMaxHours.Count > assignedHours.Count)
        //        {
        //            var remainingAssignments = assignmentsWithMaxHours.Skip(assignedHours.Count).ToList();
        //            decimal totalRemainingMax = remainingAssignments.Sum(a => a.MaxHours);
        //            if (totalRemainingMax > 0)
        //            {
        //                foreach (var assignment in remainingAssignments)
        //                {
        //                    decimal proportionalHours = (remainingHours * assignment.MaxHours) / totalRemainingMax;
        //                    decimal assigned = Math.Min(assignment.MaxHours, proportionalHours);
        //                    assignment.TaskAssignment.PlannedHours = assigned;
        //                    remainingHours -= assigned;

        //                    await _repo.Update(assignment.TaskAssignment);

        //                    if (remainingHours <= 0) break;
        //                }
        //            }
        //        }

        //        // Calculate total cost based on assigned hours and hourly rates
        //        decimal totalCost = 0m;
        //        foreach (var assignment in assignmentsWithMaxHours)
        //        {
        //            var member = projectMembers.FirstOrDefault(m => m.AccountId == assignment.TaskAssignment.AccountId);
        //            if (member?.HourlyRate.HasValue == true)
        //            {
        //                decimal memberCost = assignment.TaskAssignment.PlannedHours * member.HourlyRate.Value;
        //                totalCost += memberCost;
        //            }
        //        }

        //        task.PlannedResourceCost = totalCost;
        //        task.PlannedCost = totalCost;
        //    }

        //    // Update task with new planned hours and costs
        //    task.PlannedHours = plannedHours;
        //    task.RemainingHours = plannedHours - (task.ActualHours ?? 0m);
        //    task.UpdatedAt = DateTime.UtcNow;
        //    await _taskRepo.Update(task);
        //    await UpdateTaskProgressAsync(task);

        //    // Log the activity
        //    await _activityLogService.LogAsync(new ActivityLog
        //    {
        //        ProjectId = task.ProjectId,
        //        TaskId = taskId,
        //        SubtaskId = null,
        //        RelatedEntityType = "Task",
        //        RelatedEntityId = taskId,
        //        ActionType = "UPDATE",
        //        Message = $"Updated planned hours for task '{taskId}' to {plannedHours} and distributed to assignments",
        //        CreatedBy = createdBy,
        //        CreatedAt = DateTime.UtcNow
        //    });
        //}

        public async Task<TaskAssignmentResponseDTO> ChangeAssignmentPlannedHours(int id, decimal plannedHours, int createdBy)
        {
            var assignment = await _repo.GetByIdAsync(id);
            if (assignment == null)
                throw new KeyNotFoundException($"TaskAssignment with ID {id} not found.");

            var task = await _taskRepo.GetByIdAsync(assignment.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {assignment.TaskId} not found.");

            var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, task.ProjectId);
            if (member == null || !member.WorkingHoursPerDay.HasValue)
                throw new InvalidOperationException($"Member for account {assignment.AccountId} not found or working hours not set.");

            if (plannedHours > member.WorkingHoursPerDay.Value)
                throw new ArgumentException($"Planned hours cannot exceed member's working hours per day ({member.WorkingHoursPerDay.Value}).");

            assignment.PlannedHours = plannedHours;
            await _repo.Update(assignment);

            // Recalculate task totals
            var allAssignments = await _repo.GetByTaskIdAsync(assignment.TaskId);
            decimal totalPlannedHours = allAssignments.Sum(a => a.PlannedHours ?? 0);
            decimal totalPlannedResourceCost = 0m;

            foreach (var ass in allAssignments)
            {
                var mem = await _projectMemberRepo.GetByAccountAndProjectAsync(ass.AccountId, task.ProjectId);
                if (mem != null && mem.HourlyRate.HasValue)
                {
                    totalPlannedResourceCost += (ass.PlannedHours ?? 0) * mem.HourlyRate.Value;
                }
            }

            var actualHours = task.ActualHours ?? 0;
            task.RemainingHours = totalPlannedHours - actualHours;
            task.PlannedHours = totalPlannedHours;
            task.PlannedResourceCost = totalPlannedResourceCost;
            task.PlannedCost = totalPlannedResourceCost;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepo.Update(task);
            //await UpdateTaskProgressAsync(task);

            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                SubtaskId = null,
                RelatedEntityType = "TaskAssignment",
                RelatedEntityId = id.ToString(),
                ActionType = "UPDATE",
                Message = $"Updated planned hours for task assignment '{id}' to {plannedHours} in task '{task.Id}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            return _mapper.Map<TaskAssignmentResponseDTO>(assignment);
        }

        public async Task<List<TaskAssignmentResponseDTO>> UpdateAssignmentPlannedHoursBulk(string taskId, List<TaskAssignmentUpdateDTO> updates, int createdBy)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");

            var allAssignments = await _repo.GetByTaskIdAsync(taskId);
            if (allAssignments == null || !allAssignments.Any())
                throw new KeyNotFoundException($"No assignments found for task with ID {taskId}.");

            // Log allAssignments and updates
            Console.WriteLine($"allAssignments for task {taskId}: {System.Text.Json.JsonSerializer.Serialize(allAssignments, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles })}");
            Console.WriteLine($"Received updates: {System.Text.Json.JsonSerializer.Serialize(updates)}");

            // Filter out invalid assignments
            allAssignments = allAssignments.Where(a => a.Id > 0).ToList();
            if (!allAssignments.Any())
                throw new KeyNotFoundException($"No valid assignments (Id > 0) found for task with ID {taskId}.");

            // Validate and update each assignment
            var updatedAssignments = new List<TaskAssignment>();
            foreach (var update in updates)
            {
                var assignment = allAssignments.FirstOrDefault(a => a.Id == update.AssignmentId);
                if (assignment == null)
                    throw new KeyNotFoundException($"Assignment with ID {update.AssignmentId} not found.");

                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, task.ProjectId);
                if (member == null || !member.WorkingHoursPerDay.HasValue)
                    throw new InvalidOperationException($"Member for account {assignment.AccountId} not found or working hours not set.");

                //if (update.PlannedHours > member.WorkingHoursPerDay.Value)
                //    throw new ArgumentException($"Planned hours for assignment {update.AssignmentId} cannot exceed member's working hours per day ({member.WorkingHoursPerDay.Value}).");

                assignment.PlannedHours = update.PlannedHours;
                updatedAssignments.Add(assignment);
            }

            await _repo.UpdateRange(updatedAssignments);

            // Recalculate task totals
            decimal totalPlannedHours = allAssignments.Sum(a => a.PlannedHours ?? 0);
            decimal totalPlannedResourceCost = 0m;

            foreach (var ass in allAssignments)
            {
                var mem = await _projectMemberRepo.GetByAccountAndProjectAsync(ass.AccountId, task.ProjectId);
                if (mem != null && mem.HourlyRate.HasValue)
                {
                    totalPlannedResourceCost += (ass.PlannedHours ?? 0) * mem.HourlyRate.Value;
                }
            }

            var actualHours = task.ActualHours ?? 0;
            task.RemainingHours = totalPlannedHours - actualHours;
            task.PlannedHours = totalPlannedHours;
            task.PlannedResourceCost = totalPlannedResourceCost;
            task.PlannedCost = totalPlannedResourceCost;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepo.Update(task);
            //await UpdateTaskProgressAsync(task);

            // Log the bulk update
            await _activityLogService.LogAsync(new ActivityLog
            {
                ProjectId = task.ProjectId,
                TaskId = task.Id,
                SubtaskId = null,
                RelatedEntityType = "TaskAssignment",
                RelatedEntityId = string.Join(",", updates.Select(u => u.AssignmentId)),
                ActionType = "BULK_UPDATE",
                Message = $"Updated planned hours for multiple task assignments in task '{task.Id}'",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            // Map to DTO without cycles
            var result = _mapper.Map<List<TaskAssignmentResponseDTO>>(updatedAssignments);
            Console.WriteLine($"Returning response: {System.Text.Json.JsonSerializer.Serialize(result)}");
            return result;
        }
    }
}
