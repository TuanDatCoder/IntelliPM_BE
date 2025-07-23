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

        public async Task AddAsync(RiskSolution solution)
        {
            _context.RiskSolution.Add(solution);
            await _context.SaveChangesAsync();
        }

        public async Task<RiskSolution> GetByIdAsync(int id)
        {
            return await _context.RiskSolution.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RiskSolution?> GetByRiskIdAsync(int riskId)
        {
            return await _context.RiskSolution.FirstOrDefaultAsync(x => x.RiskId == riskId);
        }

        public async Task<List<RiskSolution>> GetListByRiskIdAsync(int riskId)
        {
            return await _context.RiskSolution
                .Where(rs => rs.RiskId == riskId)
                .ToListAsync();
        }


        public async Task UpdateAsync(RiskSolution solution)
        {
            _context.RiskSolution.Update(solution);
            await _context.SaveChangesAsync();
        }
    }
}
