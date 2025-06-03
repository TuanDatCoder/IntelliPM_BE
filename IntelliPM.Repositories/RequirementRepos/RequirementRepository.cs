using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RequirementRepos
{
    public class RequirementRepository : IRequirementRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RequirementRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Requirement>> GetAllRequirements(int projectId)
        {
            return await _context.Requirement
                .Where(r => r.ProjectId == projectId)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<Requirement?> GetByIdAsync(int id)
        {
            return await _context.Requirement
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Requirement>> GetByTitleAsync(string title)
        {
            return await _context.Requirement
                .Where(r => r.Title.Contains(title))
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task Add(Requirement requirement)
        {
            await _context.Requirement.AddAsync(requirement);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Requirement requirement)
        {
            _context.Requirement.Update(requirement);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Requirement requirement)
        {
            _context.Requirement.Remove(requirement);
            await _context.SaveChangesAsync();
        }
    }
}
