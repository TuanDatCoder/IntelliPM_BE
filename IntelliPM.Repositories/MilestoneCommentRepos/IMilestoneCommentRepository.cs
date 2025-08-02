using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MilestoneCommentRepos
{
    public interface IMilestoneCommentRepository
    {
        Task Add(MilestoneComment milestoneComment);
        Task Delete(MilestoneComment milestoneComment);
        Task<List<MilestoneComment>> GetAllMilestoneComment();
        Task<MilestoneComment?> GetByIdAsync(int id);
        Task Update(MilestoneComment milestoneComment);
        Task<List<MilestoneComment>> GetMilestoneCommentByMilestoneIdAsync(int milestoneId);
    }
}
