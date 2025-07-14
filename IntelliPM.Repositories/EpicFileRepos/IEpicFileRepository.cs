using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicFileRepos
{
    public interface IEpicFileRepository
    {
        Task AddAsync(EpicFile epicFile);
        Task<EpicFile?> GetByIdAsync(int id);
        Task DeleteAsync(EpicFile epicFile);
        Task<List<EpicFile>> GetFilesByEpicIdAsync(string epicId);
    }
}
