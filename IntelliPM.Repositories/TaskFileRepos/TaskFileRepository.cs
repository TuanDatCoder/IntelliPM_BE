using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskFileRepos
{
    public class TaskFileRepository : ITaskFileRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskFileRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TaskFile taskFile)
        {
            _context.TaskFile.Add(taskFile);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskFile?> GetByIdAsync(int id)
        {
            return await _context.TaskFile.FindAsync(id);
        }

        public async Task DeleteAsync(TaskFile taskFile)
        {
            _context.TaskFile.Remove(taskFile);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskFile>> GetFilesByTaskIdAsync(int taskId)
        {
            return await _context.TaskFile
                .Where(tf => tf.TaskId == taskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

    }

}
