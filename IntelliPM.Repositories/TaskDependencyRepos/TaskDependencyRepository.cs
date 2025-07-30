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

        public async Task DeleteByIdAsync(int id)
        {
            var dependencies = await _context.TaskDependency
                .Where(dep => dep.Id == id)
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
            var taskIds = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => t.Id)
                .ToListAsync();

            var milestoneKeys = await _context.Milestone
                .Where(m => m.ProjectId == projectId)
                .Select(m => m.Key)
                .ToListAsync();

            var subtaskIds = await _context.Subtask
                .Where(s => taskIds.Contains(s.TaskId))
                .Select(s => s.Id)
                .ToListAsync();

            return await _context.TaskDependency
                .Where(d =>
                    (taskIds.Contains(d.LinkedFrom) || taskIds.Contains(d.LinkedTo)) ||
                    (subtaskIds.Contains(d.LinkedFrom) || subtaskIds.Contains(d.LinkedTo)) ||
                    (milestoneKeys.Contains(d.LinkedFrom) || milestoneKeys.Contains(d.LinkedTo))
                )
                .ToListAsync();
        }

        public async Task<bool> ValidateItemExistsAsync(string type, string id)
        {
            return type switch
            {
                "Task" => await _context.Tasks.AnyAsync(t => t.Id == id),
                "Subtask" => await _context.Subtask.AnyAsync(s => s.Id == id),
                "Milestone" => await _context.Milestone.AnyAsync(m => m.Key == id),
                _ => false
            };
        }

        public async Task Add(TaskDependency taskDependency)
        {
            await _context.TaskDependency.AddAsync(taskDependency);
            await _context.SaveChangesAsync();
        }


    }
}
