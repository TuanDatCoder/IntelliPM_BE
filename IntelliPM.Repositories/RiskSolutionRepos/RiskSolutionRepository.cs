using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
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
    }
}
