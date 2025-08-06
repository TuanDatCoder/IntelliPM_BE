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
            var dateOnly = logDate.Date;

            return await _context.WorkLog.AnyAsync(w =>
                w.LogDate.Date == dateOnly &&
                ((taskId != null && w.TaskId == taskId) ||
                 (subtaskId != null && w.SubtaskId == subtaskId))
            );
        }

        public async Task BulkInsertAsync(List<WorkLog> logs)
        {
            await _context.WorkLog.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkLog>> GetByTaskOrSubtaskIdAsync(string? taskId, string? subtaskId)
        {
            return await _context.WorkLog   
                .Where(w => (taskId != null && w.TaskId == taskId) ||
                            (subtaskId != null && w.SubtaskId == subtaskId))
                .OrderByDescending(w => w.Id)
                .ToListAsync();
        }

        public async Task<WorkLog> GetByIdAsync(int id)
        {
            return await _context.WorkLog
                .Include(s => s.Task)
                .Include(v => v.Subtask)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task Update(WorkLog workLog)
        {
            _context.WorkLog.Update(workLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkLog>> GetBySubtaskIdAsync(string subtaskId)
        {
            return await _context.WorkLog
                .Where(w => w.SubtaskId == subtaskId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }
    }
}
