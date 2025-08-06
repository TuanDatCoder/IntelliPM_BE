using IntelliPM.Data.DTOs.Task.Response;
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
        Task<List<Tasks>> GetByEpicIdAsync(string epicId);
        Task<List<Tasks>> GetBySprintIdAsync(int sprintId);
        Task Add(Tasks task);
        Task Update(Tasks task);
        Task Delete(Tasks task);
        Task<string?> GetProjectKeyByTaskIdAsync(string taskId);
        Task<List<Tasks>> GetInProgressAsync();
        Task<TaskWithSubtaskDTO?> GetTaskWithSubtasksAsync(string id);
        Task<List<Tasks>> GetBySprintIdAndByStatusAsync(int sprintId, string status);
        Task AddRangeAsync(List<Tasks> tasks);
        Task SaveChangesAsync();


    }
}
