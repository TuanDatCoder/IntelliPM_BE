using IntelliPM.Data.DTOs.ActivityLog.Response;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ActivityLogServices
{
    public interface IActivityLogService
    {
        Task LogAsync(ActivityLog log);
        Task LogFieldChangeAsync(string entityType, string? entityId, int projectId, string field, string? oldValue, string? newValue, string actionType, int userId, string? taskId = null, string? subtaskId = null);
        Task<List<ActivityLogResponseDTO>> GetAllActivityLogList();
        Task<ActivityLogResponseDTO> GetActivityLogById(int id);
        Task<List<ActivityLogResponseDTO>> GetActivityLogsByProjectId(int projectId);
        Task<List<ActivityLogResponseDTO>> GetActivityLogsByTaskId(string taskId);
        Task<List<ActivityLogResponseDTO>> GetActivityLogsBySubtaskId(string subtaskId);
    }
}
