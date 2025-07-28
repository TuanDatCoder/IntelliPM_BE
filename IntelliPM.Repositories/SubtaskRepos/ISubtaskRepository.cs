using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskRepos
{
    public interface ISubtaskRepository
    {
        Task<List<Subtask>> GetAllSubtask();
        Task<Subtask?> GetByIdAsync(string id);
        Task Add(Subtask subtask);
        Task Update(Subtask subtask);
        Task Delete(Subtask subtask);
        Task<List<Subtask>> GetSubtaskByTaskIdAsync(string taskId);
        Task<List<Subtask>> GetInProgressAsync();
        Task<List<Subtask>> GetByAccountIdAsync(int id);

    }
}
