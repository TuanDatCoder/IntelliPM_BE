using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.LabelRepos
{
    public class LabelRepository : ILabelRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public LabelRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(Label label)
        {
            await _context.Label.AddAsync(label);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Label label)
        {
            _context.Label.Remove(label);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Label>> GetAllLabelAsync()
        {
            return await _context.Label
                .Include(l => l.Project)
                .Include(l => l.WorkItemLabel)
                .OrderBy(l => l.Id)
                .ToListAsync();
        }

        public async Task<Label?> GetByIdAsync(int id)
        {
            return await _context.Label
                .Include(l => l.Project)
                .Include(l => l.WorkItemLabel)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Label>> GetByProjectAsync(int projectId)
        {
            return await _context.Label
                .Where(tf => tf.ProjectId == projectId)
                .Include(l => l.Project)
                .Include(l => l.WorkItemLabel)
                .ToListAsync();
        }

        public async Task Update(Label label)
        {
            _context.Label.Update(label);
            await _context.SaveChangesAsync();
        }
    }
}
