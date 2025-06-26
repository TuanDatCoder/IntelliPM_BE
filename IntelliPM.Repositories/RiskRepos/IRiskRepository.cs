using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskRepos
{
    public interface IRiskRepository
    {
        Task<List<Risk>> GetAllRisksAsync();
        Task<List<Risk>> GetByProjectIdAsync(int projectId);
        Task<Risk?> GetByIdAsync(int id);
        Task AddAsync(Risk risk);
        Task UpdateAsync(Risk risk);
        Task DeleteAsync(Risk risk);
    }
}
