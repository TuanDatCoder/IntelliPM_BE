using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskCommentRepos
{
    public class TaskCommentRepository : ITaskCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }
        public async Task Add(TaskComment taskComment)
        {
            await _context.TaskComment.AddAsync(taskComment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TaskComment taskComment)
        {
            _context.TaskComment.Remove(taskComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskComment>> GetAllTaskComment()
        {
            return await _context.TaskComment
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<TaskComment?> GetByIdAsync(int id)
        {
            return await _context.TaskComment
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Update(TaskComment taskComment)
        {
            _context.TaskComment.Update(taskComment);
            await _context.SaveChangesAsync();
        }
    }
}
