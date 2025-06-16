using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MilestoneRepos
{
    public interface IMilestoneRepository
    {
        Task<List<Milestone>> GetAllMilestones();
        Task<Milestone?> GetByIdAsync(int id);
        Task<List<Milestone>> GetByNameAsync(string name);
        Task<List<Milestone>> GetMilestonesByProjectIdAsync(int projectId);
        Task Add(Milestone milestone);
        Task Update(Milestone milestone);
        Task Delete(Milestone milestone);
    }
}
