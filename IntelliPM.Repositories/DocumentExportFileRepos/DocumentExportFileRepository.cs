using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.DocumentExportFileRepos
{
    public class DocumentExportFileRepository : IDocumentExportFileRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentExportFileRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DocumentExportFile entity)
        {
            await _context.DocumentExportFile.AddAsync(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<DocumentExportFile?> GetByFileUrlAsync(string fileUrl)
        {
            return await _context.DocumentExportFile
                .Include(e => e.Document)
                .FirstOrDefaultAsync(e => e.ExportedFileUrl == fileUrl);
        }
    }
}
