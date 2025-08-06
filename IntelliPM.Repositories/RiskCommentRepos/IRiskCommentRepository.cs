using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskCommentRepos
{
    public interface IRiskCommentRepository
    {
        Task<List<RiskComment>> GetAllRiskComment();
        Task<RiskComment?> GetByIdAsync(int id);
        Task Add(RiskComment riskComment);
        Task Update(RiskComment riskComment);
        Task Delete(RiskComment riskComment);
        Task<List<RiskComment>> GetByRiskIdAsync(int riskId);
    }
}
