using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicFileRepos
{
    public class EpicFileRepository : IEpicFileRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public EpicFileRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EpicFile epicFile)
        {
            _context.EpicFile.Add(epicFile);
            await _context.SaveChangesAsync();
        }

        public async Task<EpicFile?> GetByIdAsync(int id)
        {
            return await _context.EpicFile.FindAsync(id);
        }

        public async Task DeleteAsync(EpicFile epicFile)
        {
            _context.EpicFile.Remove(epicFile);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EpicFile>> GetFilesByEpicIdAsync(string epicId)
        {
            return await _context.EpicFile
                .Where(tf => tf.EpicId == epicId)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }
    }
}