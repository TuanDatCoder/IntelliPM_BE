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

        public async Task<List<Document>> GetAllAsync()
            => await _context.Document
                             .Where(d => d.IsActive)
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

        public async Task<List<Document>> GetByStatusAsync(string status)
        {
            return await _context.Document
                .Where(d => d.Status == status && d.IsActive)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByStatusAndProjectAsync(string status, int projectId)
        {
            return await _context.Document
                .Where(d => d.Status == status && d.ProjectId == projectId && d.IsActive)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByEpicIdAsync(string epicId)
        {
            return await _context.Document
                .Where(d => d.EpicId == epicId && d.IsActive)
                .ToListAsync();
        }
        public async Task<List<Document>> GetByTaskIdAsync(string taskId)
        {
            return await _context.Document
                .Where(d => d.TaskId == taskId && d.IsActive)
                .ToListAsync();
        }
        public async Task<List<Document>> GetBySubtaskIdAsync(string subtaskId)
        {
            return await _context.Document
                .Where(d => d.SubtaskId == subtaskId && d.IsActive)
                .ToListAsync();
        }

        public async Task<List<Document>> GetByProjectAndTaskAsync(int projectId, string taskId)
        {
            return await _context.Document
                .Where(d => d.ProjectId == projectId && d.TaskId == taskId && d.IsActive)
                .ToListAsync();
        }

        public async Task<Document?> GetByKeyAsync(int projectId, string? epicId, string? taskId, string? subTaskId)
        {
            string? keyId = epicId ?? taskId ?? subTaskId;

            if (keyId == null)
                return null;

            return await _context.Document
                .Where(d => d.ProjectId == projectId &&
                            d.TaskId == keyId &&
                            d.IsActive)
                .FirstOrDefaultAsync();
        }
        public async Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId)
        {
            var docs = await _context.Document
                .Where(d => d.ProjectId == projectId && d.CreatedBy == userId)
                .ToListAsync();

            return docs
                .Where(d => d.EpicId != null || d.TaskId != null || d.SubtaskId != null)
                .ToDictionary(
                    d => d.EpicId ?? d.TaskId ?? d.SubtaskId!,
                    d => d.Id
                );
        }

        public async Task<Dictionary<string, int>> CountByStatusAsync()
        {
            return await _context.Document
                .Where(d => d.IsActive)
                .GroupBy(d => d.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<Dictionary<string, int>> CountByStatusInProjectAsync(int projectId)
        {
            return await _context.Document
                .Where(d => d.IsActive && d.ProjectId == projectId)
                .GroupBy(d => d.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<List<int>> GetAccountIdsByEmailsAsync(List<string> emails)
        {
            return await _context.Account
                .Where(a => emails.Contains(a.Email))
                .Select(a => a.Id)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetAccountMapByEmailsAsync(List<string> emails)
        {
            return await _context.Account
                .Where(a => emails.Contains(a.Email))
                .ToDictionaryAsync(a => a.Email.ToLower(), a => a.Id);
        }





    }
}
