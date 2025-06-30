using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.ProjectMetricRepos
{
    public class ProjectMetricRepository : IProjectMetricRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ProjectMetricRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task Add(ProjectMetric metric)
        {
            await _context.ProjectMetric.AddAsync(metric);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProjectMetric>> GetAllAsync()
        {
            return await _context.ProjectMetric.ToListAsync();
        }

        public async Task<ProjectMetric?> GetByIdAsync(int id)
        {
            return await _context.ProjectMetric.FindAsync(id);
        }

        //public async Task<List<ProjectMetric>> GetByProjectIdAsync(int projectId)
        //{
        //    return await _context.ProjectMetric
        //        .Where(m => m.ProjectId == projectId)
        //        .ToListAsync();
        //}

        public async Task<ProjectMetric?> GetLatestByProjectIdAsync(int projectId)
        {
            return await _context.ProjectMetric
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task Update(ProjectMetric metric)
        {
            _context.ProjectMetric.Update(metric);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectMetric?> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectMetric
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId);
        }
    }
}
