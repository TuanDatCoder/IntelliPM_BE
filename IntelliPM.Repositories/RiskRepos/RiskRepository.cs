using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskRepos
{
    public class RiskRepository : IRiskRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RiskRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Risk risk)
        {
            await _context.Risk.AddAsync(risk);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Risk risk)
        {
            _context.Risk.Remove(risk);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Risk>> GetAllRisksAsync()
        {
            return await _context.Risk
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public async Task<Risk?> GetByIdAsync(int id)
        {
            return await _context.Risk
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Risk>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Risk
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Risk risk)
        {
            _context.Risk.Update(risk);
            await _context.SaveChangesAsync();
        }
    }
}
