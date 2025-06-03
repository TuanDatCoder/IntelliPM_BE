using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MilestoneRepos
{
    public class MilestoneRepository : IMilestoneRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MilestoneRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Milestone>> GetAllMilestones()
        {
            return await _context.Milestone
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<Milestone?> GetByIdAsync(int id)
        {
            return await _context.Milestone
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Milestone>> GetByNameAsync(string name)
        {
            return await _context.Milestone
                .Where(m => m.Name.Contains(name))
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        public async Task Add(Milestone milestone)
        {
            await _context.Milestone.AddAsync(milestone);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Milestone milestone)
        {
            _context.Milestone.Update(milestone);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Milestone milestone)
        {
            _context.Milestone.Remove(milestone);
            await _context.SaveChangesAsync();
        }
    }
}
