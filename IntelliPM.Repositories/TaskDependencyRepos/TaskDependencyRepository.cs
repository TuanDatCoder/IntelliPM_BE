using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskDependencyRepos
{
    public class TaskDependencyRepository : ITaskDependencyRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskDependencyRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task DeleteByTaskIdAsync(string taskId)
        {
            var dependencies = await _context.TaskDependency
                .Where(dep => dep.TaskId == taskId)
                .ToListAsync();

            _context.TaskDependency.RemoveRange(dependencies);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<TaskDependency> dependencies)
        {
            _context.TaskDependency.AddRange(dependencies);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskDependency>> GetByProjectIdAsync(int projectId)
        {
            return await _context.TaskDependency
                .Where(d => _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .Select(t => t.Id)
                    .Contains(d.TaskId))
                .ToListAsync();
        }

        public async Task<List<TaskDependency>> GetByTaskIdAsync(string taskId)
        {
            return await _context.TaskDependency
                .Where(d => d.TaskId == taskId)
                .ToListAsync();
        }

    }
}
