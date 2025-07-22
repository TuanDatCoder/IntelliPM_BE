using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SprintRepos
{
    public interface ISprintRepository
    {
        Task<List<Sprint>> GetAllSprints();
        Task<Sprint?> GetByIdAsync(int id);
        Task<List<Sprint>> GetByNameAsync(string name);
        Task<List<Sprint>> GetByProjectIdAsync(int projectId);
        Task Add(Sprint sprint);
        Task Update(Sprint sprint);
        Task Delete(Sprint sprint);

        Task<List<Sprint>> GetByProjectIdDescendingAsync(int projectId);
    }
}
