using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicCommentRepos
{
    public class EpicCommentRepository : IEpicCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public EpicCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(EpicComment epicComment)
        {
            if (epicComment == null) throw new ArgumentNullException(nameof(epicComment));
            await _context.EpicComment.AddAsync(epicComment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(EpicComment epicComment)
        {
            if (epicComment == null) throw new ArgumentNullException(nameof(epicComment));
            _context.EpicComment.Remove(epicComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EpicComment>> GetAllEpicComment()
        {
            return await _context.EpicComment
                .Include(e => e.Epic)
                .Include(e => e.Account)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<EpicComment?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid ID", nameof(id));
            return await _context.EpicComment
                .Include(e => e.Epic)
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task Update(EpicComment epicComment)
        {
            if (epicComment == null) throw new ArgumentNullException(nameof(epicComment));
            _context.EpicComment.Update(epicComment);
            await _context.SaveChangesAsync();
        }
    }
}
