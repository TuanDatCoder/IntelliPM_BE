using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.WorkItemLabelRepos
{
    public class WorkItemLabelRepository : IWorkItemLabelRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public WorkItemLabelRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(WorkItemLabel workItemLabel)
        {
            if (workItemLabel == null) throw new ArgumentNullException(nameof(workItemLabel));
            await _context.WorkItemLabel.AddAsync(workItemLabel);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(WorkItemLabel workItemLabel)
        {
            if (workItemLabel == null) throw new ArgumentNullException(nameof(workItemLabel));
            _context.WorkItemLabel.Remove(workItemLabel);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkItemLabel>> GetAllWorkItemLabelAsync()
        {
            return await _context.WorkItemLabel
                .Include(w => w.Label)
                .Include(w => w.Epic)
                .Include(w => w.Label)
                .Include(w => w.Subtask)
                .Include(w => w.Task)
                .OrderBy(w => w.Id)
                .ToListAsync();
        }

        public async Task<WorkItemLabel?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid ID", nameof(id));
            return await _context.WorkItemLabel
                .Include(w => w.Label)
                .Include(w => w.Epic)
                .Include(w => w.Label)
                .Include(w => w.Subtask)
                .Include(w => w.Task)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task Update(WorkItemLabel workItemLabel)
        {
            if (workItemLabel == null) throw new ArgumentNullException(nameof(workItemLabel));
            _context.WorkItemLabel.Update(workItemLabel);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkItemLabel>> GetByEpicIdAsync(string? epicId)
        {
            if (string.IsNullOrEmpty(epicId)) return await GetAllWorkItemLabelAsync();
            return await _context.WorkItemLabel
                .Include(w => w.Label)
                .Include(w => w.Epic)
                .Include(w => w.Label)
                .Include(w => w.Subtask)
                .Include(w => w.Task)
                .Where(w => w.EpicId == epicId)
                .ToListAsync();
        }

        public async Task<List<WorkItemLabel>> GetBySubtaskIdAsync(string? subtaskId)
        {
            if (string.IsNullOrEmpty(subtaskId)) return await GetAllWorkItemLabelAsync();
            return await _context.WorkItemLabel
                .Include(w => w.Label)
                .Include(w => w.Epic)
                .Include(w => w.Label)
                .Include(w => w.Subtask)
                .Include(w => w.Task)
                .Where(w => w.SubtaskId == subtaskId)
                .ToListAsync();
        }

        public async Task<List<WorkItemLabel>> GetByTaskIdAsync(string? taskId)
        {
            if (string.IsNullOrEmpty(taskId)) return await GetAllWorkItemLabelAsync();
            return await _context.WorkItemLabel
                .Include(w => w.Label)
                .Include(w => w.Epic)
                .Include(w => w.Label)
                .Include(w => w.Subtask)
                .Include(w => w.Task)
                .Where(w => w.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<bool> IsLabelAlreadyAssignedAsync(int labelId, string? taskId, string? epicId, string? subtaskId)
        {
            return await _context.WorkItemLabel.AnyAsync(l =>
                l.LabelId == labelId &&
                l.TaskId == taskId &&
                l.EpicId == epicId &&
                l.SubtaskId == subtaskId &&
                !l.IsDeleted);
        }
    }
}
