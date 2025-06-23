using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskRepos
{
    public interface ITaskRepository
    {
        Task<List<Tasks>> GetAllTasks();
        Task<Tasks?> GetByIdAsync(string id);
        Task<List<Tasks>> GetByTitleAsync(string title);
        Task<List<Tasks>> GetByProjectIdAsync(int projectId);
        Task Add(Tasks task);
        Task Update(Tasks task);
        Task Delete(Tasks task);
        Task<string?> GetProjectKeyByTaskIdAsync(string taskId);

    }
}
