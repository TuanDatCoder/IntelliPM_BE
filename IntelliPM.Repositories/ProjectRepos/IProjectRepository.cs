using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectRepos
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjects();
        Task<Project?> GetByIdAsync(int id);
        Task<List<Project>> SearchProjects(string searchTerm, string? projectType, string? status);
        Task Add(Project project);
        Task Update(Project project);
        Task Delete(Project project);
    }
}
