using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SprintRepos
{
    public class SprintRepository : ISprintRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public SprintRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Sprint>> GetAllSprints()
        {
            return await _context.Sprint
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Sprint?> GetByIdAsync(int id)
        {
            return await _context.Sprint
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Sprint>> GetByNameAsync(string name)
        {
            return await _context.Sprint
                .Where(s => s.Name.Contains(name))
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task Add(Sprint sprint)
        {
            await _context.Sprint.AddAsync(sprint);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Sprint sprint)
        {
            _context.Sprint.Update(sprint);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Sprint sprint)
        {
            _context.Sprint.Remove(sprint);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Sprint>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Sprint
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }
    }
}
