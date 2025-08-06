using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.AiResponseEvaluationRepos
{
    public class AiResponseEvaluationRepository : IAiResponseEvaluationRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public AiResponseEvaluationRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<AiResponseEvaluation>> GetAllAsync()
        {
            return await _context.AiResponseEvaluation
                .Include(e => e.Account)
                .Include(e => e.AiResponse)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<AiResponseEvaluation?> GetByIdAsync(int id)
        {
            return await _context.AiResponseEvaluation
                .Include(e => e.Account)
                .Include(e => e.AiResponse)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<AiResponseEvaluation>> GetByAiResponseIdAsync(int aiResponseId)
        {
            return await _context.AiResponseEvaluation
                .Include(e => e.Account)
                .Include(e => e.AiResponse)
                .Where(e => e.AiResponseId == aiResponseId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AiResponseEvaluation>> GetByAccountIdAsync(int accountId)
        {
            return await _context.AiResponseEvaluation
                .Include(e => e.Account)
                .Include(e => e.AiResponse)
                .Where(e => e.AccountId == accountId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(AiResponseEvaluation aiResponseEvaluation)
        {
            await _context.AiResponseEvaluation.AddAsync(aiResponseEvaluation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AiResponseEvaluation aiResponseEvaluation)
        {
            _context.AiResponseEvaluation.Update(aiResponseEvaluation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AiResponseEvaluation aiResponseEvaluation)
        {
            _context.AiResponseEvaluation.Remove(aiResponseEvaluation);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
