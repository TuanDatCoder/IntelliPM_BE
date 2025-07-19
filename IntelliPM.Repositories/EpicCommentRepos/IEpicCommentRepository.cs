using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicCommentRepos
{
    public interface IEpicCommentRepository
    {
        Task Add(EpicComment epicComment);
        Task Delete(EpicComment epicComment);
        Task<List<EpicComment>> GetAllEpicComment();
        Task<EpicComment?> GetByIdAsync(int id);
        Task Update(EpicComment epicComment);
        Task<List<EpicComment>> GetEpicCommentByEpicIdAsync(string epicId);
    }
}
