using AutoMapper;
using CloudinaryDotNet;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.WorkLog.Request;
using IntelliPM.Data.DTOs.WorkLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Repositories.WorkLogRepos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.WorkLogServices
{
    public class WorkLogService : IWorkLogService
    {
        private readonly IMapper _mapper;
        private readonly IWorkLogRepository _workLogRepo;
        private readonly ILogger<WorkLogService> _logger;
        private readonly ITaskRepository _taskRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;

        public WorkLogService(IMapper mapper, IWorkLogRepository workLogRepo, ILogger<WorkLogService> logger, ITaskRepository taskRepo, ISubtaskRepository subtaskRepo, IProjectMemberRepository projectMemberRepo, IAccountRepository accountRepo, ITaskAssignmentRepository taskAssignmentRepo)
        {
            _mapper = mapper;
            _workLogRepo = workLogRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _subtaskRepo = subtaskRepo;
            _projectMemberRepo = projectMemberRepo;
            _accountRepo = accountRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
        }

        public async Task<List<WorkLogResponseDTO>> ChangeMultipleWorkLogHoursAsync(Dictionary<int, decimal> updates)
        {
            var results = new List<WorkLogResponseDTO>();
            var affectedSubtaskIds = new HashSet<string>();

            foreach (var update in updates)
            {
                int id = update.Key;
                decimal hours = update.Value;

                var entity = await _workLogRepo.GetByIdAsync(id);
                if (entity == null)
                    continue;

                entity.Hours = hours;
                entity.UpdatedAt = DateTime.UtcNow;
                await _workLogRepo.Update(entity);

                if (entity.SubtaskId != null)
                {
                    affectedSubtaskIds.Add(entity.SubtaskId);
                }

                results.Add(_mapper.Map<WorkLogResponseDTO>(entity));
            }

            // Sau khi cập nhật tất cả work logs, cập nhật lại các subtasks bị ảnh hưởng
            foreach (var subtaskId in affectedSubtaskIds)
            {
                var allWorkLogs = await _workLogRepo.GetBySubtaskIdAsync(subtaskId);
                var totalHours = allWorkLogs.Sum(w => w.Hours ?? 0);

                var subtask = await _subtaskRepo.GetByIdAsync(subtaskId);
                if (subtask != null)
                {
                    subtask.ActualHours = totalHours;
                    subtask.UpdatedAt = DateTime.UtcNow;

                    // Tính ActualResourceCost
                    if (subtask.AssignedBy != null)
                    {
                        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
                        if (task?.ProjectId != null)
                        {
                            var member = await _projectMemberRepo.GetByAccountAndProjectAsync(subtask.AssignedBy.Value, task.ProjectId);
                            if (member != null && member.HourlyRate.HasValue)
                            {
                                subtask.ActualResourceCost = subtask.ActualHours * member.HourlyRate.Value;
                                subtask.ActualCost = subtask.ActualHours * member.HourlyRate.Value;
                            }
                        }
                    }
                    await _subtaskRepo.Update(subtask);
                    await UpdateSubtaskProgressAsync(subtask);

                    // Cập nhật task nếu có
                    if (subtask.TaskId != null)
                    {
                        var allSubtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(subtask.TaskId);
                        var totalSubtaskHours = allSubtasks.Sum(s => s.ActualHours ?? 0);
                        var totalActualResourceCost = allSubtasks.Sum(s => s.ActualResourceCost ?? 0);

                        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
                        if (task != null)
                        {
                            var plannedHours = task.PlannedHours;
                            task.RemainingHours = plannedHours - totalSubtaskHours;
                            task.ActualHours = totalSubtaskHours;
                            task.ActualResourceCost = totalActualResourceCost;
                            task.ActualCost = totalActualResourceCost;
                            task.UpdatedAt = DateTime.UtcNow;
                            await _taskRepo.Update(task);
                            await UpdateTaskProgressBySubtasksAsync(task.Id);
                        }
                    }
                }
            }

            return results;
        }

        private async Task UpdateSubtaskProgressAsync(Subtask subtask)
        {
            if (subtask.Status == "DONE")
            {
                subtask.PercentComplete = 100;
            }
            else if (subtask.Status == "TO_DO")
            {
                subtask.PercentComplete = 0;
            }
            else if (subtask.Status == "IN_PROGRESS")
            {
                if (subtask.PlannedHours > 0)
                {
                    var rawProgress = (subtask.ActualHours / subtask.PlannedHours) * 100;
                    subtask.PercentComplete = Math.Min((int)rawProgress, 99);
                }
                else
                {
                    subtask.PercentComplete = 0;
                }
            }

            subtask.UpdatedAt = DateTime.UtcNow;
            await _subtaskRepo.Update(subtask);
        }

        private async Task UpdateTaskProgressBySubtasksAsync(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return;

            var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

            if (!subtasks.Any())
            {
                task.PercentComplete = 0;
            }
            else
            {
                var avg = subtasks.Average(st => st.PercentComplete ?? 0);
                task.PercentComplete = (int)Math.Round(avg);
            }

            task.UpdatedAt = DateTime.UtcNow;
            await _taskRepo.Update(task);
        }

        public async Task<WorkLogResponseDTO> ChangeWorkLogHoursAsync(int id, decimal hours)
        {
            var entity = await _workLogRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"WorkLog with ID {id} not found.");
            entity.Hours = hours;
            entity.UpdatedAt = DateTime.UtcNow;

            await _workLogRepo.Update(entity);

            // cập nhật Subtask.ActualHours
            if (entity.SubtaskId != null)
            {
                var allWorkLogs = await _workLogRepo.GetBySubtaskIdAsync(entity.SubtaskId);
                var totalHours = allWorkLogs.Sum(w => w.Hours ?? 0);

                var subtask = await _subtaskRepo.GetByIdAsync(entity.SubtaskId);
                if (subtask != null)
                {
                    subtask.ActualHours = totalHours;
                    subtask.UpdatedAt = DateTime.UtcNow;

                    // Tính ActualResourceCost của subtask
                    if (subtask.AssignedBy != null)
                    {
                        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
                        if (task?.ProjectId != null)
                        {
                            var member = await _projectMemberRepo.GetByAccountAndProjectAsync(subtask.AssignedBy.Value, task.ProjectId);
                            if (member != null && member.HourlyRate.HasValue)
                            {
                                subtask.ActualResourceCost = subtask.ActualHours * member.HourlyRate.Value;
                            }
                        }
                    }
                    await _subtaskRepo.Update(subtask);

                    // Cập nhật Task.ActualHours = tổng ActualHours của tất cả subtasks
                    if (subtask.TaskId != null)
                    {
                        var allSubtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(subtask.TaskId);
                        var totalSubtaskHours = allSubtasks.Sum(s => s.ActualHours ?? 0);
                        var totalActualResourceCost = allSubtasks.Sum(s => s.ActualResourceCost ?? 0);

                        var task = await _taskRepo.GetByIdAsync(subtask.TaskId);
                        if (task != null)
                        {
                            var plannedHours = task.PlannedHours;
                            task.RemainingHours = plannedHours - totalSubtaskHours;
                            task.ActualHours = totalSubtaskHours;
                            task.ActualResourceCost = totalActualResourceCost;
                            task.ActualCost = totalActualResourceCost;
                            task.UpdatedAt = DateTime.UtcNow;
                            await _taskRepo.Update(task);
                        }
                    }
                }
            }

            return _mapper.Map<WorkLogResponseDTO>(entity);
        }

        public async Task GenerateDailyWorkLogsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var logsToInsert = new List<WorkLog>();

            // Tasks đang in-progress
            var inProgressTasks = await _taskRepo.GetInProgressAsync();
            foreach (var task in inProgressTasks)
            {
                var exists = await _workLogRepo.ExistsAsync(task.Id, null, today);
                if (!exists)
                {
                    logsToInsert.Add(new WorkLog
                    {
                        TaskId = task.Id,
                        LogDate = today,
                        Hours = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // Subtasks đang in-progress
            var inProgressSubtasks = await _subtaskRepo.GetInProgressAsync();
            foreach (var subtask in inProgressSubtasks)
            {
                var exists = await _workLogRepo.ExistsAsync(null, subtask.Id, today);
                if (!exists)
                {
                    logsToInsert.Add(new WorkLog
                    {
                        SubtaskId = subtask.Id,
                        LogDate = today,
                        Hours = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            if (logsToInsert.Any())
            {
                await _workLogRepo.BulkInsertAsync(logsToInsert);
            }
        }

        public async Task<List<WorkLogResponseDTO>> GetWorkLogsByTaskOrSubtaskAsync(string? taskId, string? subtaskId)
        {
            var worklogs = await _workLogRepo.GetByTaskOrSubtaskIdAsync(taskId, subtaskId);
            var result = _mapper.Map<List<WorkLogResponseDTO>>(worklogs)
                .OrderByDescending(x => x.Id)
                .ToList();

            if (!string.IsNullOrEmpty(subtaskId))
            {
                var subtask = await _subtaskRepo.GetByIdAsync(subtaskId);
                if (subtask?.AssignedBy != null)
                {
                    var account = await _accountRepo.GetAccountById(subtask.AssignedBy.Value);
                    foreach (var log in result)
                    {
                        log.Accounts = new List<AccountHourBasicDTO>
                {
                    new AccountHourBasicDTO
                    {
                        Id = account.Id,
                        FullName = account.FullName,
                        Username = account.Username
                    }
                };
                    }
                }
            }
            else if (!string.IsNullOrEmpty(taskId))
            {
                var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);
                var accountIds = assignments.Select(x => x.AccountId).Distinct().ToList();
                var accounts = await _accountRepo.GetByIdsAsync(accountIds);

                var accountHourMap = assignments
                    .GroupBy(x => x.AccountId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.ActualHours ?? 0));

                var accountDTOs = accounts.Select(a => new AccountHourBasicDTO
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Username = a.Username,
                    Hours = accountHourMap.ContainsKey(a.Id) ? accountHourMap[a.Id] : 0
                }).ToList();

                foreach (var log in result)
                {
                    log.Accounts = accountDTOs;
                }
            }

            return result;
        }

        //public async Task<bool> UpdateWorkLogByAccountsAsync(UpdateWorkLogByAccountsDTO dto)
        //{
        //    var workLog = await _workLogRepo.GetByIdAsync(dto.WorkLogId);
        //    if (workLog == null) throw new Exception("Work log not found");

        //    // Cập nhật tổng số giờ
        //    var totalHours = dto.Entries.Sum(e => e.Hours);
        //    workLog.Hours = totalHours;
        //    await _workLogRepo.Update(workLog);

        //    // Cập nhật actualHours cho task
        //    var task = await _taskRepo.GetByIdAsync(dto.TaskId);
        //    if (task == null) throw new Exception("Task not found");

        //    task.ActualHours = totalHours;

        //    // Tính actual_resource_cost
        //    var projectId = task.ProjectId;
        //    decimal totalCost = 0;

        //    foreach (var entry in dto.Entries)
        //    {
        //        var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(projectId, entry.AccountId);
        //        if (projectMember == null) continue;

        //        totalCost += entry.Hours * (projectMember.HourlyRate ?? 0);
        //    }

        //    task.ActualResourceCost = totalCost;

        //    await _taskRepo.Update(task);

        //    return true;
        //}

        //public async Task<bool> UpdateWorkLogsByAccountsAsync(UpdateWorkLogsByAccountsDTO dto)
        //{
        //    decimal totalHours = 0;
        //    decimal totalCost = 0;

        //    foreach (var logDto in dto.WorkLogs)
        //    {
        //        var workLog = await _workLogRepo.GetByIdAsync(logDto.WorkLogId);
        //        if (workLog == null) continue;

        //        var logTotalHours = logDto.Entries.Sum(e => e.Hours);
        //        workLog.Hours = logTotalHours;
        //        await _workLogRepo.Update(workLog);
        //        totalHours += logTotalHours;

        //        var task = await _taskRepo.GetByIdAsync(dto.TaskId);
        //        if (task == null) throw new Exception("Task not found");

        //        var projectId = task.ProjectId;

        //        foreach (var entry in logDto.Entries)
        //        {
        //            var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(projectId, entry.AccountId);
        //            if (projectMember == null) continue;

        //            totalCost += entry.Hours * (projectMember.HourlyRate ?? 0);
        //        }
        //    }

        //    // Cập nhật task
        //    var taskToUpdate = await _taskRepo.GetByIdAsync(dto.TaskId);
        //    if (taskToUpdate != null)
        //    {
        //        taskToUpdate.ActualHours = totalHours;
        //        taskToUpdate.ActualResourceCost = totalCost;
        //        await _taskRepo.Update(taskToUpdate);
        //    }

        //    return true;
        //}

        public async Task<bool> UpdateWorkLogsByAccountsAsync(UpdateWorkLogsByAccountsDTO dto)
        {
            decimal totalHours = 0;
            // var accountHours = new Dictionary<int, decimal>(); // <accountId, totalHours>

            foreach (var logDto in dto.WorkLogs)
            {
                var workLog = await _workLogRepo.GetByIdAsync(logDto.WorkLogId);
                if (workLog == null) continue;

                var logTotalHours = logDto.Entries.Sum(e => e.Hours);
                workLog.Hours = logTotalHours;
                await _workLogRepo.Update(workLog);

                totalHours += logTotalHours;
            }

            // Gom tổng số giờ theo từng account
            var accountHours = dto.WorkLogs
                .SelectMany(wl => wl.Entries)
                .GroupBy(e => e.AccountId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));

            // Tính tổng chi phí theo accountId
            decimal totalCost = 0;
            var task = await _taskRepo.GetByIdAsync(dto.TaskId);
            if (task == null) throw new Exception("Task not found");

            foreach (var kvp in accountHours)
            {
                Console.WriteLine($"AccountId: {kvp.Key}, Hours: {kvp.Value}");
                var accountId = kvp.Key;
                var hours = kvp.Value;

                var projectMember = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, task.ProjectId);
                if (projectMember == null) continue;

                totalCost += hours * (projectMember.HourlyRate ?? 0);
                Console.WriteLine($"Account {accountId}: {hours}h * {projectMember.HourlyRate ?? 0} = {hours * (projectMember.HourlyRate ?? 0)}");

                var assignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(dto.TaskId, accountId);
                if (assignment != null)
                {
                    assignment.ActualHours = hours;
                    await _taskAssignmentRepo.Update(assignment);
                }
            }

            // Cập nhật task
            var plannedHours = task.PlannedHours;
            task.RemainingHours = plannedHours - totalHours;
            task.ActualHours = totalHours;
            task.ActualResourceCost = totalCost;
            task.ActualCost = totalCost;
            await _taskRepo.Update(task);
            await UpdateTaskProgressAsync(task);

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
                if (task.PlannedHours.HasValue && task.PlannedHours > 0)
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
