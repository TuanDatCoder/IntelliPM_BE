using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;
        public ActivityLogRepository(Su25Sep490IntelliPmContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public Su25Sep490IntelliPmContext GetContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<Su25Sep490IntelliPmContext>();
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
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetByTaskIdAsync(string taskId)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .Where(t => t.TaskId == taskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetByEpicIdAsync(string epicId)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .Where(t => t.EpicId == epicId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }


        public async Task<List<ActivityLog>> GetBySubtaskIdAsync(string subtaskId)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .Where(t => t.SubtaskId == subtaskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task AddRangeAsync(List<ActivityLog> activityLogs)
        {
            await _context.ActivityLog.AddRangeAsync(activityLogs);
        }

        public async Task<List<ActivityLog>> GetByRiskKeyAsync(string riskKey)
        {
            return await _context.ActivityLog
                .Include(s => s.CreatedByNavigation)
                .Where(t => t.RiskKey == riskKey)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}
