using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RequirementRepos
{
    public interface IRequirementRepository
    {
        Task<List<Requirement>> GetAllRequirements(int projectId);
        Task<Requirement?> GetByIdAsync(int id);
        Task<List<Requirement>> GetByTitleAsync(string title);
        Task Add(Requirement requirement);
        Task Update(Requirement requirement);
        Task Delete(Requirement requirement);
    }
}
