using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskCommentRepos
{
    public interface ITaskCommentRepository
    {
        Task<List<TaskComment>> GetAllTaskComment();
        Task<TaskComment?> GetByIdAsync(int id);
        Task Add(TaskComment taskComment);
        Task Update(TaskComment taskComment);
        Task Delete(TaskComment taskComment);
        Task<List<TaskComment>> GetTaskCommentByTaskIdAsync(string taskId);

    }
}
