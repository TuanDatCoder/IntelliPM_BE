using AutoMapper;
using IntelliPM.Data.DTOs.WorkLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.SubtaskRepos;
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

        public WorkLogService(IMapper mapper, IWorkLogRepository workLogRepo, ILogger<WorkLogService> logger, ITaskRepository taskRepo, ISubtaskRepository subtaskRepo, IProjectMemberRepository projectMemberRepo)
        {
            _mapper = mapper;
            _workLogRepo = workLogRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _subtaskRepo = subtaskRepo;
            _projectMemberRepo = projectMemberRepo;
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
                            task.ActualHours = totalSubtaskHours;
                            task.ActualResourceCost = totalActualResourceCost;
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
            var list = await _workLogRepo.GetByTaskOrSubtaskIdAsync(taskId, subtaskId);
            return _mapper.Map<List<WorkLogResponseDTO>>(list);
        }
    }
}
