using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
                .Where(ta => ta.Account.Id == accountId)
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

        public async Task<List<TaskAssignment>> GetByProjectIdAsync(int projectId)
        {
            return await _context.TaskAssignment
                .Include(ta => ta.Task)
                .Where(ta => ta.Task.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<TaskAssignment>> GetByTaskIdAndAccountIdAsync(string taskId, int accountId)
        {
            return await _context.TaskAssignment
               .Where(x => x.TaskId == taskId && x.AccountId == accountId)
               .Include(ta => ta.Account)
               .Include(ta => ta.Task)
               .ToListAsync();
        }

        public async Task<List<TaskAssignment>> GetTasksByAccountIdAsync(int accountId)
        {
            return await _context.TaskAssignment
                .Where(ta => ta.AccountId == accountId)
                .Include(ta => ta.Task)
                .OrderByDescending(ta => ta.Task.CreatedAt)
                .ToListAsync();
        }



    }
}
