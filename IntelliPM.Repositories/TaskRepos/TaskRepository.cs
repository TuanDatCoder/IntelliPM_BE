﻿using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskRepos
{
    public class TaskRepository : ITaskRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Tasks>> GetAllTasks()
        {
            return await _context.Tasks
                .Include(v => v.Project)
                .Include(a => a.Reporter)
                .Include(e => e.Sprint)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<Tasks?> GetByIdAsync(string id)
        {
            return await _context.Tasks
                .Include(v => v.Project)
                .Include(a => a.Reporter)
                .Include(e => e.Sprint)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Tasks>> GetByTitleAsync(string title)
        {
            return await _context.Tasks
                .Where(t => t.Title.Contains(title))
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task Add(Tasks task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Tasks task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Tasks task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Tasks>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Tasks
                .Include(v => v.Project)
                .Include(a => a.Reporter)
                .Include(e => e.Epic)
                .Include(e => e.Sprint)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<List<Tasks>> GetByEpicIdAsync(string epicId)
        {
            return await _context.Tasks
                .Include(v => v.Project)
                .Include(a => a.Reporter)
                .Include(e => e.Sprint)
                .Where(t => t.EpicId == epicId)
                .ToListAsync();
        }

        public async Task<List<Tasks>> GetBySprintIdAsync(int sprintId)
        {
            return await _context.Tasks
                .Include(v => v.Project)
                .Include(a => a.Reporter)
                .Include(e => e.Sprint)
                .Where(t => t.SprintId == sprintId)
                .ToListAsync();
        }

        public async Task<string> GetProjectKeyByTaskIdAsync(string taskId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) throw new Exception($"Task with ID {taskId} not found.");

            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == task.ProjectId);
            if (project == null) throw new Exception($"Project not found for Task {taskId}.");

            return project.ProjectKey;
        }

        public async Task<List<Tasks>> GetInProgressAsync()
        {
            return await _context.Tasks
                .Where(t => t.Status == "IN_PROGRESS")
                .ToListAsync();
        }

        public async Task<TaskWithSubtaskDTO?> GetTaskWithSubtasksAsync(string id)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskAssignment).ThenInclude(ta => ta.Account)
                .Include(t => t.Subtask)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return null;

            return new TaskWithSubtaskDTO
            {
                Id = task.Id,
                PlannedHours = task.PlannedHours,
                ActualHours = task.ActualHours,
                Accounts = task.TaskAssignment.Select(ta => new AccountBasicDTO
                {
                    Id = ta.Account.Id,
                    Username = ta.Account.Username,
                    FullName = ta.Account.FullName
                }).ToList(),
                Subtasks = task.Subtask.Select(st => new SubtaskBasicDTO
                {
                    Id = st.Id,
                    TaskId = st.TaskId,
                    AssignedBy = (int)st.AssignedBy,
                    PlannedHours = st.PlannedHours,
                    ActualHours = st.ActualHours
                }).ToList()
            };
        }

    }
}
