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

        public async Task<List<TaskAssignmentHourDTO>> GetTaskAssignmentHoursByTaskIdAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.");

            var entities = await _repo.GetByTaskIdAsync(taskId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No task assignments found for Task ID {taskId}.");

            return _mapper.Map<List<TaskAssignmentHourDTO>>(entities);
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
                await UpdateTaskProgressAsync(task);
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

        private async Task UpdateTaskProgressAsync(Tasks task)
        {
            if (task.Status == "DONE")
            {
                task.PercentComplete = 100;
            }
            else if (task.Status == "TO_DO")
            {
                task.PercentComplete = 0;
            }
            else if (task.Status == "IN_PROGRESS")
            {
                if (task.PlannedHours > 0)
                {
                    var rawProgress = (task.ActualHours / task.PlannedHours) * 100;
                    task.PercentComplete = Math.Min((int)rawProgress, 99);
                }
                else
                {
                    task.PercentComplete = 0;
                }
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.Update(task);
        }


    }
}
