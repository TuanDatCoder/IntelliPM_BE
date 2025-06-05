using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectPositionRepos
{
    public interface IProjectPositionRepository
    {
        Task<List<ProjectPosition>> GetAllProjectPositions(int projectMemberId);
        Task<ProjectPosition?> GetByIdAsync(int id);
        Task Add(ProjectPosition projectPosition);
        Task Update(ProjectPosition projectPosition);
        Task Delete(ProjectPosition projectPosition);
    }
}
