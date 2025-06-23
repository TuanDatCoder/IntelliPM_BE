using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskAssignmentRepos
{
    public class TaskAssignmentRepository : ITaskAssignmentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskAssignmentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<TaskAssignment>> GetAllAsync()
        {
            return await _context.TaskAssignment
                .Include(ta => ta.Account)
                .Include(ta => ta.Task)
                .ToListAsync();
        }

        public async Task<List<TaskAssignment>> GetByTaskIdAsync(string taskId)
        {
            return await _context.TaskAssignment
                .Where(ta => ta.TaskId == taskId)
                .Include(ta => ta.Account)
                .Include(ta => ta.Task)
                .ToListAsync();
        }

        public async Task<List<TaskAssignment>> GetByAccountIdAsync(int accountId)
        {
            return await _context.TaskAssignment
                .Where(ta => ta.AccountId == accountId)
                .Include(ta => ta.Account)
                .Include(ta => ta.Task)
                .ToListAsync();
        }

        public async Task<TaskAssignment> GetByIdAsync(int id)
        {
            return await _context.TaskAssignment
                .Include(ta => ta.Account)
                .Include(ta => ta.Task)
                .FirstOrDefaultAsync(ta => ta.Id == id);
        }

        public async Task Add(TaskAssignment taskAssignment)
        {
            _context.TaskAssignment.Add(taskAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TaskAssignment taskAssignment)
        {
            _context.TaskAssignment.Update(taskAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TaskAssignment taskAssignment)
        {
            _context.TaskAssignment.Remove(taskAssignment);
            await _context.SaveChangesAsync();
        }
    }
}
