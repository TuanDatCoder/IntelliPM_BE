using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.WorkLogRepos
{
    public interface IWorkLogRepository
    {
        Task<bool> ExistsAsync(string? taskId, string? subtaskId, DateTime logDate);
        Task BulkInsertAsync(List<WorkLog> logs);

    }
}
