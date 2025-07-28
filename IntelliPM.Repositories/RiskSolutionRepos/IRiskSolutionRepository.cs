using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskSolutionRepos
{
    public interface IRiskSolutionRepository
    {
        Task<List<RiskSolution>> GetByRiskIdAsync(int riskId);
        Task<RiskSolution?> GetByIdAsync(int id);
        Task AddAsync(RiskSolution entity);
        Task UpdateAsync(RiskSolution entity);
        Task Delete(RiskSolution entity);
    }
}
