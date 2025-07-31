using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskDependencyRepos
{
    public interface ITaskDependencyRepository
    {
        Task<List<TaskDependency>> GetByProjectIdAsync(int projectId);
        //Task<List<TaskDependency>> GetByTaskIdAsync(string taskId);
        Task DeleteByIdAsync(int id);
        Task AddRangeAsync(List<TaskDependency> dependencies);
        Task SaveChangesAsync();
        Task Add(TaskDependency taskDependency);
        Task<List<TaskDependency>> GetDependenciesByLinkedFromAsync(string linkedFrom);
        Task AddMany(List<TaskDependency> dependencies);
        Task UpdateMany(List<TaskDependency> dependencies);
        Task<IEnumerable<TaskDependency>> FindAllAsync(Expression<Func<TaskDependency, bool>> predicate);

    }

}
