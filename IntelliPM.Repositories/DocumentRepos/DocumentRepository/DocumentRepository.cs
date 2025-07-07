using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.DocumentRepos.DocumentRepository
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetByProjectAsync(int projectId)
            => await _context.Document
                             .Where(d => d.ProjectId == projectId && d.IsActive)
                             .ToListAsync();

        public async Task<Document?> GetByIdAsync(int id)
            => await _context.Document.FindAsync(id);

        public async Task AddAsync(Document doc)
            => await _context.Document.AddAsync(doc);

        public async Task UpdateAsync(Document doc)
            => _context.Document.Update(doc);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task<List<Document>> GetByUserIdAsync(int userId)
        {
            return await _context.Document
                .Where(d => d.CreatedBy == userId && d.IsActive)
                .ToListAsync();
        }

        //public async Task<List<Document>> GetByStatusAsync(string status)
        //{
        //    return await _context.Document
        //        .Where(d => d. == status && d.IsActive)
        //        .ToListAsync();
        //}

        //public async Task<List<Document>> GetByStatusAndProjectAsync(string status, int projectId)
        //{
        //    return await _context.Document
        //        .Where(d => d.Status == status && d.ProjectId == projectId && d.IsActive)
        //        .ToListAsync();
        //}



    }
}
