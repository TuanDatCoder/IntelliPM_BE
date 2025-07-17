using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.WorkLogRepos
{
    public class WorkLogRepository : IWorkLogRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public WorkLogRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string? taskId, string? subtaskId, DateTime logDate)
        {
            return await _context.WorkLog.AnyAsync(w =>
                w.LogDate.Date == logDate &&
                ((taskId != null && w.TaskId == taskId) ||
                 (subtaskId != null && w.SubtaskId == subtaskId))
            );
        }

        public async Task BulkInsertAsync(List<WorkLog> logs)
        {
            await _context.WorkLog.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

    }
}
