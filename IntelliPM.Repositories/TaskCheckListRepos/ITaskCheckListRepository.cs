using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskCheckListRepos
{
    public interface ITaskCheckListRepository
    {
        Task<List<TaskCheckList>> GetAllTaskCheckList();
        Task<TaskCheckList?> GetByIdAsync(int id);
        Task Add(TaskCheckList taskCheckList);
        Task Update(TaskCheckList taskCheckList);
        Task Delete(TaskCheckList taskCheckList);
    }
}
