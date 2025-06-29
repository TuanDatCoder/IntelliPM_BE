using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskCommentRepos
{
    public interface ISubtaskCommentRepository
    {
        Task<List<SubtaskComment>> GetAllSubtaskComment();
        Task<SubtaskComment?> GetByIdAsync(int id);
        Task Add(SubtaskComment subtaskComment);
        Task Update(SubtaskComment subtaskComment);
        Task Delete(SubtaskComment subtaskComment);
        Task<List<SubtaskComment>> GetSubtaskCommentBySubtaskIdAsync(string subtaskId);
    }
}
