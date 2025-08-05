using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MilestoneCommentRepos
{
    public class MilestoneCommentRepository : IMilestoneCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MilestoneCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task Add(MilestoneComment milestoneComment)
        {
            await _context.MilestoneComment.AddAsync(milestoneComment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(MilestoneComment milestoneComment)
        {
            _context.MilestoneComment.Remove(milestoneComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MilestoneComment>> GetAllMilestoneComment()
        {
            return await _context.MilestoneComment
                .Include(m => m.Milestone)
                .Include(m => m.Account)
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<MilestoneComment?> GetByIdAsync(int id)
        {
            return await _context.MilestoneComment
                .Include(m => m.Milestone)
                .Include(m => m.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task Update(MilestoneComment milestoneComment)
        {
            _context.MilestoneComment.Update(milestoneComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MilestoneComment>> GetMilestoneCommentByMilestoneIdAsync(int milestoneId)
        {
            return await _context.MilestoneComment
                .Where(m => m.MilestoneId == milestoneId)
                .Include(m => m.Account)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
