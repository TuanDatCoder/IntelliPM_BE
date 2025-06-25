using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskFileRepos
{
    public interface ISubtaskFileRepository
    {
        Task AddAsync(SubtaskFile subtaskFile);
        Task<SubtaskFile?> GetByIdAsync(int id);
        Task DeleteAsync(SubtaskFile subtaskFile);
        Task<List<SubtaskFile>> GetFilesBySubtaskIdAsync(string subtaskId);
    }
}
