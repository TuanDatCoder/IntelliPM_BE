using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskCommentRepos
{
    public class RiskCommentRepository : IRiskCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RiskCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task Add(RiskComment riskComment)
        {
            await _context.RiskComment.AddAsync(riskComment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(RiskComment riskComment)
        {
            _context.RiskComment.Remove(riskComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RiskComment>> GetAllRiskComment()
        {
            return await _context.RiskComment
                .OrderBy(t => t.Id)
                .Include(t => t.Account)
                .ToListAsync();
        }

        public async Task<RiskComment?> GetByIdAsync(int id)
        {
            return await _context.RiskComment
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Update(RiskComment riskComment)
        {
            _context.RiskComment.Update(riskComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RiskComment>> GetByRiskIdAsync(int riskId)
        {
            return await _context.RiskComment
                .Where(tf => tf.RiskId == riskId)
                .Include(t => t.Account)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}
