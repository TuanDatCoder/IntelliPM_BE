using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.WorkItemLabelRepos
{
    public interface IWorkItemLabelRepository
    {
        Task Add(WorkItemLabel workItemLabel);
        Task Delete(WorkItemLabel workItemLabel);
        Task<List<WorkItemLabel>> GetAllWorkItemLabelAsync();
        Task<WorkItemLabel?> GetByIdAsync(int id);
        Task Update(WorkItemLabel workItemLabel);
        Task<List<WorkItemLabel>> GetByEpicIdAsync(string? epicId);
        Task<List<WorkItemLabel>> GetBySubtaskIdAsync(string? subtaskId);
        Task<List<WorkItemLabel>> GetByTaskIdAsync(string? taskId);
    }
}
