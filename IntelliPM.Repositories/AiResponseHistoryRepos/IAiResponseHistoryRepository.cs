using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.AiResponseHistoryRepos
{
    public interface IAiResponseHistoryRepository
    {
        Task<List<AiResponseHistory>> GetAllAsync();
        Task<AiResponseHistory?> GetByIdAsync(int id);
        Task<List<AiResponseHistory>> GetByAiFeatureAsync(string aiFeature);
        Task<List<AiResponseHistory>> GetByProjectIdAsync(int projectId);
        Task<List<AiResponseHistory>> GetByCreatedByAsync(int createdBy);
        Task AddAsync(AiResponseHistory aiResponseHistory);
        Task UpdateAsync(AiResponseHistory aiResponseHistory);
        Task DeleteAsync(AiResponseHistory aiResponseHistory);
        Task SaveChangesAsync();
    }
}
