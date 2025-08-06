using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskSolutionRepos
{
    public class RiskSolutionRepository : IRiskSolutionRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RiskSolutionRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<RiskSolution>> GetByRiskIdAsync(int riskId)
        {
            return await _context.RiskSolution
                .Where(rs => rs.RiskId == riskId)
                .OrderBy(rs => rs.Id)
                .ToListAsync();
        }

        public async Task<RiskSolution?> GetByIdAsync(int id)
        {
            return await _context.RiskSolution
                .FirstOrDefaultAsync(rs => rs.Id == id);
        }

        public async Task AddAsync(RiskSolution entity)
        {
            _context.RiskSolution.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RiskSolution entity)
        {
            _context.RiskSolution.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(RiskSolution entity)
        {
            _context.RiskSolution.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
