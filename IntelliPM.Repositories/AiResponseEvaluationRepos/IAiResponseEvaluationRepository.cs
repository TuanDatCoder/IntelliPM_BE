using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.AiResponseEvaluationRepos
{
    public interface IAiResponseEvaluationRepository
    {
        Task<List<AiResponseEvaluation>> GetAllAsync();
        Task<AiResponseEvaluation?> GetByIdAsync(int id);
        Task<List<AiResponseEvaluation>> GetByAiResponseIdAsync(int aiResponseId);
        Task<List<AiResponseEvaluation>> GetByAccountIdAsync(int accountId);
        Task AddAsync(AiResponseEvaluation aiResponseEvaluation);
        Task UpdateAsync(AiResponseEvaluation aiResponseEvaluation);
        Task DeleteAsync(AiResponseEvaluation aiResponseEvaluation);
        Task SaveChangesAsync();
    }
}
