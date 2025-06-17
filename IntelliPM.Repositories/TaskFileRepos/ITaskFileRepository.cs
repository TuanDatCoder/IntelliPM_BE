using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskFileRepos
{
    public interface ITaskFileRepository
    {
        Task AddAsync(TaskFile taskFile);
        Task<TaskFile?> GetByIdAsync(int id);
        Task DeleteAsync(TaskFile taskFile);
        Task<List<TaskFile>> GetFilesByTaskIdAsync(string taskId);

    }
}
