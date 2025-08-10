using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.AiResponseHistoryRepos
{
    public class AiResponseHistoryRepository : IAiResponseHistoryRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public AiResponseHistoryRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<AiResponseHistory>> GetAllAsync()
        {
            return await _context.AiResponseHistory
                .Include(r => r.Project)
                .Include(r => r.CreatedByNavigation)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<AiResponseHistory?> GetByIdAsync(int id)
        {
            return await _context.AiResponseHistory
                .Include(r => r.Project)
                .Include(r => r.CreatedByNavigation)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<AiResponseHistory>> GetByAiFeatureAsync(string aiFeature)
        {
            return await _context.AiResponseHistory
                .Include(r => r.Project)
                .Include(r => r.CreatedByNavigation)
                .Where(r => r.AiFeature == aiFeature)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AiResponseHistory>> GetByProjectIdAsync(int projectId)
        {
            return await _context.AiResponseHistory
                .Include(r => r.Project)
                .Include(r => r.CreatedByNavigation)
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AiResponseHistory>> GetByCreatedByAsync(int createdBy)
        {
            return await _context.AiResponseHistory
                .Include(r => r.Project)
                .Include(r => r.CreatedByNavigation)
                .Where(r => r.CreatedBy == createdBy)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(AiResponseHistory aiResponseHistory)
        {
            await _context.AiResponseHistory.AddAsync(aiResponseHistory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AiResponseHistory aiResponseHistory)
        {
            _context.AiResponseHistory.Update(aiResponseHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AiResponseHistory aiResponseHistory)
        {
            _context.AiResponseHistory.Remove(aiResponseHistory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
