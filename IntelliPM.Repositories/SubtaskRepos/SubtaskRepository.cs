using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskRepos
{
    public class SubtaskRepository : ISubtaskRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public SubtaskRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Subtask>> GetAllSubtask()
        {
            return await _context.Subtask
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<Subtask?> GetByIdAsync(string id)
        {
            return await _context.Subtask
                .Include(s => s.Task) 
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task Add(Subtask subtask)
        {
            await _context.Subtask.AddAsync(subtask);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Subtask subtask)
        {
            _context.Subtask.Update(subtask);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Subtask subtask)
        {
            _context.Subtask.Remove(subtask);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Subtask>> GetSubtaskByTaskIdAsync(string taskId)
        {
            return await _context.Subtask
                .Where(tf => tf.TaskId == taskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}
