using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.SubtaskFileRepos
{
    public class SubtaskFileRepository : ISubtaskFileRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public SubtaskFileRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SubtaskFile subtaskFile)
        {
            _context.SubtaskFile.Add(subtaskFile);
            await _context.SaveChangesAsync();
        }

        public async Task<SubtaskFile?> GetByIdAsync(int id)
        {
            return await _context.SubtaskFile.FindAsync(id);
        }

        public async Task DeleteAsync(SubtaskFile subtaskFile)
        {
            _context.SubtaskFile.Remove(subtaskFile);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SubtaskFile>> GetFilesBySubtaskIdAsync(string subtaskId)
        {
            return await _context.SubtaskFile
                .Where(tf => tf.SubtaskId == subtaskId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}

