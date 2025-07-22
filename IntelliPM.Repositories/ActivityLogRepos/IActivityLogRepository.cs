using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ActivityLogRepos
{
    public interface IActivityLogRepository
    {
        Task Add(ActivityLog activityLog);
        Task<List<ActivityLog>> GetAllActivityLog();
        Task<ActivityLog?> GetByIdAsync(int id);
        Task<List<ActivityLog>> GetByProjectIdAsync(int projectId);

        Task<List<ActivityLog>> GetByTaskIdAsync(string taskId);

        Task<List<ActivityLog>> GetBySubtaskIdAsync(string subtaskId);
    }
}
