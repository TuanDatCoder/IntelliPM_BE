using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskAssignmentRepos
{
    public interface ITaskAssignmentRepository
    {
        Task<List<TaskAssignment>> GetAllAsync();
        Task<List<TaskAssignment>> GetByTaskIdAsync(string taskId);
        Task<List<TaskAssignment>> GetByAccountIdAsync(int accountId);
        Task<TaskAssignment> GetByIdAsync(int id);
        Task Add(TaskAssignment taskAssignment);
        Task Update(TaskAssignment taskAssignment);
        Task Delete(TaskAssignment taskAssignment);
    }
}
