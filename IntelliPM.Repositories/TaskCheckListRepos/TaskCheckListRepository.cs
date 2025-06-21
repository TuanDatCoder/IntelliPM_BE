using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskCheckListRepos
{
    public class TaskCheckListRepository : ITaskCheckListRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public TaskCheckListRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<TaskCheckList>> GetAllTaskCheckList()
        {
            return await _context.TaskCheckList
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<TaskCheckList?> GetByIdAsync(int id)
        {
            return await _context.TaskCheckList
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Add(TaskCheckList taskCheckList)
        {
            await _context.TaskCheckList.AddAsync(taskCheckList);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TaskCheckList taskCheckList)
        {
            _context.TaskCheckList.Update(taskCheckList);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TaskCheckList taskCheckList)
        {
            _context.TaskCheckList.Remove(taskCheckList);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskCheckList>> GetTaskCheckListByTaskIdAsync(string taskId)
        {
            return await _context.TaskCheckList
                .Where(tf => tf.TaskId == taskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}
