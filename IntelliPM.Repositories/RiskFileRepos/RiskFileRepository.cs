using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RiskFileRepos
{
    public class RiskFileRepository : IRiskFileRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RiskFileRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RiskFile riskFile)
        {
            _context.RiskFile.Add(riskFile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RiskFile riskFile)
        {
            _context.RiskFile.Remove(riskFile);
            await _context.SaveChangesAsync();
        }

        public async Task<RiskFile?> GetByIdAsync(int id)
        {
            return await _context.RiskFile.FindAsync(id);
        }

        public async Task<List<RiskFile>> GetByRiskIdAsync(int riskId)
        {
            return await _context.RiskFile
                .Where(tf => tf.RiskId == riskId)
                .OrderByDescending(tf => tf.UploadedAt)
                .ToListAsync();
        }
    }
}
