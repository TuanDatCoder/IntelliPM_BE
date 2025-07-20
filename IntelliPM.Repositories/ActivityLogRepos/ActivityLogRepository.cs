using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ActivityLogRepos
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ActivityLogRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task Add(ActivityLog activityLog)
        {
            await _context.ActivityLog.AddAsync(activityLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetAllActivityLog()
        {
            return await _context.ActivityLog
                .Include(t => t.CreatedByNavigation)
                .ToListAsync();
        }


        public async Task<ActivityLog?> GetByIdAsync(int id)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<ActivityLog>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }
    }
}
