using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MetricHistoryRepos
{
    public class MetricHistoryRepository : IMetricHistoryRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MetricHistoryRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task Add(ProjectMetricHistory metricHistory)
        {
            await _context.ProjectMetricHistory.AddAsync(metricHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProjectMetricHistory>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectMetricHistory
                .Where(h => h.ProjectId == projectId)
                .OrderByDescending(h => h.RecordedAt)
                .ToListAsync();
        }
    }
}
