using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectRecommendationRepos
{
    public class ProjectRecommendationRepository : IProjectRecommendationRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ProjectRecommendationRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectRecommendation>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Set<ProjectRecommendation>()
                .Where(r => r.ProjectId == projectId)
                .Include(r => r.Task)
                .ToListAsync();
        }

        public async Task Add(ProjectRecommendation recommendation)
        {
            _context.Set<ProjectRecommendation>().Add(recommendation);
            await _context.SaveChangesAsync();
        }
    }

}
