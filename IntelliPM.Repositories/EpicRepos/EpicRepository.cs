using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicRepos
{
    public class EpicRepository : IEpicRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public EpicRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Epic>> GetAllEpics()
        {
            return await _context.Epic
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<Epic?> GetByIdAsync(string id)
        {
            return await _context.Epic
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Epic>> GetByNameAsync(string name)
        {
            return await _context.Epic
                .Where(e => e.Name.Contains(name))
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<List<Epic>> GetByProjectKeyAsync(string projectKey) 
        {
            return await _context.Epic
                .Where(e => e.Project != null && e.Project.ProjectKey == projectKey)
                .ToListAsync();
        }

        public async Task Add(Epic epic)
        {
            await _context.Epic.AddAsync(epic);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Epic epic)
        {
            _context.Epic.Update(epic);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Epic epic)
        {
            _context.Epic.Remove(epic);
            await _context.SaveChangesAsync();
        }
    }
}
