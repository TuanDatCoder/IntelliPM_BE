using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectPositionRepos
{
    public class ProjectPositionRepository : IProjectPositionRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ProjectPositionRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectPosition>> GetAllProjectPositions(int projectMemberId)
        {
            return await _context.ProjectPosition
                .Where(pp => pp.ProjectMemberId == projectMemberId)
                .OrderBy(pp => pp.Id)
                .ToListAsync();
        }

        public async Task<ProjectPosition?> GetByIdAsync(int id)
        {
            return await _context.ProjectPosition
                .FirstOrDefaultAsync(pp => pp.Id == id);
        }

        public async Task Add(ProjectPosition projectPosition)
        {
            await _context.ProjectPosition.AddAsync(projectPosition);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ProjectPosition projectPosition)
        {
            _context.ProjectPosition.Update(projectPosition);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ProjectPosition projectPosition)
        {
            _context.ProjectPosition.Remove(projectPosition);
            await _context.SaveChangesAsync();
        }
    }
}
