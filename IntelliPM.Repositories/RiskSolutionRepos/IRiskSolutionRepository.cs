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
        Task AddAsync(RiskSolution solution);
        Task<RiskSolution> GetByIdAsync(int id);
        Task UpdateAsync(RiskSolution solution);
        Task<List<RiskSolution>> GetListByRiskIdAsync(int riskId);

    }
}
