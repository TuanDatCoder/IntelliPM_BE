using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskFileRepos
{
    public interface IRiskFileRepository
    {
        Task AddAsync(RiskFile riskFile);
        Task<RiskFile?> GetByIdAsync(int id);
        Task DeleteAsync(RiskFile riskFile);
        Task<List<RiskFile>> GetByRiskIdAsync(int riskId);
    }
}
