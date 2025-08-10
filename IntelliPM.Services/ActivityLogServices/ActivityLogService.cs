using AutoMapper;
using IntelliPM.Data.DTOs.ActivityLog.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ActivityLogRepos;

namespace IntelliPM.Services.ActivityLogServices
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMapper _mapper;

        public ActivityLogService(IActivityLogRepository activityLogRepository, IMapper mapper)
        {
            _activityLogRepository = activityLogRepository;
            _mapper = mapper;
        }

        public async Task<ActivityLogResponseDTO> GetActivityLogById(int id)
        {
            var entity = await _activityLogRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"ActivityLog with ID {id} not found.");

            return _mapper.Map<ActivityLogResponseDTO>(entity);
        }

        public async Task<List<ActivityLogResponseDTO>> GetAllActivityLogList()
        {
            var entities = await _activityLogRepository.GetAllActivityLog();
            return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
        }

        public async Task LogAsync(ActivityLog log)
        {
            log.CreatedAt = DateTime.UtcNow;
            await _activityLogRepository.Add(log);
        }

        public async Task<List<ActivityLogResponseDTO>> GetActivityLogsByProjectId(int projectId)
        {
            {
                var entities = await _activityLogRepository.GetByProjectIdAsync(projectId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No activityLogs found for Project ID {projectId}.");

                return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
            }
        }

        public async Task<List<ActivityLogResponseDTO>> GetActivityLogsBySubtaskId(string subtaskId)
        {
            {
                var entities = await _activityLogRepository.GetBySubtaskIdAsync(subtaskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No activityLogs found for Subtask ID {subtaskId}.");

                return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
            }
        }

        public async Task<List<ActivityLogResponseDTO>> GetActivityLogsByTaskId(string taskId)
        {
            {
                var entities = await _activityLogRepository.GetByTaskIdAsync(taskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No activityLogs found for Task ID {taskId}.");

                return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
            }
        }

        public async Task<List<ActivityLogResponseDTO>> GetActivityLogsByEpicId(string epicId)
        {
            {
                var entities = await _activityLogRepository.GetByTaskIdAsync(epicId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No activityLogs found for Epic ID {epicId}.");

                return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
            }
        }

        public async Task LogFieldChangeAsync(
            string entityType,
            string? entityId,
            int projectId,
            string field,
            string? oldValue,
            string? newValue,
            string actionType,
            int userId,
            string? taskId = null,
            string? subtaskId = null,
            string? epicId = null)
        {
            var log = new ActivityLog
            {
                ProjectId = projectId,
                TaskId = taskId,
                SubtaskId = subtaskId,
                EpicId = epicId,
                RelatedEntityType = entityType,
                RelatedEntityId = entityId,
                ActionType = actionType,
                FieldChanged = field,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _activityLogRepository.Add(log);
        }

        public async Task LogMessageAsync(
            string entityType,
            string? entityId,
            int projectId,
            string message,
            string actionType,
            int userId,
            string? taskId = null,
            string? subtaskId = null,
             string? epicId = null)
        {
            var log = new ActivityLog
            {
                ProjectId = projectId,
                TaskId = taskId,
                SubtaskId = subtaskId,
                EpicId = epicId,
                RelatedEntityType = entityType,
                RelatedEntityId = entityId,
                ActionType = actionType,
                Message = message,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _activityLogRepository.Add(log);
        }

        public async Task<List<ActivityLogResponseDTO>> GetActivityLogsByRiskKey(string riskKey)
        {
            var entities = await _activityLogRepository.GetByRiskKeyAsync(riskKey);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No activityLogs found for risk key {riskKey}.");

            return _mapper.Map<List<ActivityLogResponseDTO>>(entities);
        }
    }
}