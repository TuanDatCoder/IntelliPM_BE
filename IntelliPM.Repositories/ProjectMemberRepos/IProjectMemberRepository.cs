using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectMemberRepos
{
    public interface IProjectMemberRepository
    {
        Task<List<ProjectMember>> GetAllAsync();
        Task<List<ProjectMember>> GetAllProjectMembers(int projectId);
        Task<ProjectMember?> GetByIdAsync(int id);
        Task<ProjectMember?> GetByAccountAndProjectAsync(int accountId, int projectId);
        Task Add(ProjectMember projectMember);
        Task Update(ProjectMember projectMember);
        Task Delete(ProjectMember projectMember);


        
    }
}
