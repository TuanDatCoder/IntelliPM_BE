using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentPermissionRepos
{
    public class DocumentPermissionRepository : IDocumentPermissionRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentPermissionRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentPermission>> GetByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentPermission
                .Where(dp => dp.DocumentId == documentId)
                .ToListAsync();
        }

        public async Task<string?> GetPermissionTypeAsync(int documentId, int accountId)
        {
            return await _context.DocumentPermission
                .Where(p => p.DocumentId == documentId && p.AccountId == accountId)
                .Select(p => p.PermissionType)
                .FirstOrDefaultAsync();
        }



        public async Task AddRangeAsync(IEnumerable<DocumentPermission> permissions)
        {
            await _context.DocumentPermission.AddRangeAsync(permissions);
        }

        public void RemoveRange(IEnumerable<DocumentPermission> permissions)
        {
            _context.DocumentPermission.RemoveRange(permissions);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
