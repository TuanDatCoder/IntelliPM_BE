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

        public async Task ApproveRiskAsync(int riskId)
        {
            var risk = await _context.Risk.FindAsync(riskId);
            if (risk != null)
            {
                risk.IsApproved = true;
                risk.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
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
                .Include(r => r.RiskSolution)
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<Risk>> GetUnapprovedAIRisksByProjectIdAsync(int projectId)
        {
            return await _context.Risk
                .Where(r => r.ProjectId == projectId && r.GeneratedBy == "AI" && !r.IsApproved)
                .Include(r => r.RiskSolution)
                .ToListAsync();
        }

        public async Task UpdateAsync(Risk risk)
        {
            _context.Risk.Update(risk);
            await _context.SaveChangesAsync();
        }
    }
}
