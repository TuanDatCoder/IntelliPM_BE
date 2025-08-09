using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentCommentRepos
{
    public class DocumentCommentRepository : IDocumentCommentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentCommentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentComment>> GetByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentComment
                .Include(c => c.Author) // 👈 cần dòng này
                .Where(c => c.DocumentId == documentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }


        public async Task<DocumentComment> AddAsync(DocumentComment comment)
        {
            _context.DocumentComment.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<DocumentComment?> GetByIdAsync(int id)
        {
            return await _context.DocumentComment
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(DocumentComment comment)
        {
            _context.DocumentComment.Update(comment);
            await _context.SaveChangesAsync();
        }


        public async Task<Account?> GetAuthorByIdAsync(int authorId)
        {
            return await _context.Account.FindAsync(authorId);
        }

        public async Task DeleteAsync(DocumentComment comment)
        {
            _context.DocumentComment.Remove(comment);
            await _context.SaveChangesAsync();
        }


    }
}
