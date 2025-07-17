using AutoMapper;
using IntelliPM.Data.DTOs.WorkLog.Response;
using IntelliPM.Data.Entities;
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

        public WorkLogService(IMapper mapper, IWorkLogRepository workLogRepo, ILogger<WorkLogService> logger, ITaskRepository taskRepo, ISubtaskRepository subtaskRepo)
        {
            _mapper = mapper;
            _workLogRepo = workLogRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _subtaskRepo = subtaskRepo;
        }

        public async Task<WorkLogResponseDTO> ChangeWorkLogHoursAsync(int id, decimal hours)
        {
            var entity = await _workLogRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"WorkLog with ID {id} not found.");
            entity.Hours = hours;
            entity.UpdatedAt = DateTime.UtcNow;

            await _workLogRepo.Update(entity);
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
