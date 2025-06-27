using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectMemberRepos
{
    public class ProjectMemberRepository : IProjectMemberRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ProjectMemberRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }
        public async Task<List<ProjectMember>> GetAllAsync()
        {
            return await _context.ProjectMember
                .Include(pm => pm.Account)
                .Include(pm => pm.Project)
                .ToListAsync();
        }
        public async Task<List<ProjectMember>> GetAllProjectMembers(int projectId)
        {
            return await _context.ProjectMember
                .Where(pm => pm.ProjectId == projectId)
                .OrderBy(pm => pm.Id)
                .ToListAsync();
        }

        public async Task<ProjectMember?> GetByIdAsync(int id)
        {
            return await _context.ProjectMember
                .Include(pm => pm.Account)
                .Include(pm => pm.ProjectPosition)
                .FirstOrDefaultAsync(pm => pm.Id == id);
        }

        public async Task<ProjectMember?> GetByAccountAndProjectAsync(int accountId, int projectId)
        {
            return await _context.ProjectMember
                .Include(pm => pm.Account)
                .Include(pm => pm.ProjectPosition)
                .FirstOrDefaultAsync(pm => pm.AccountId == accountId && pm.ProjectId == projectId);
        }
        public async Task Add(ProjectMember projectMember)
        {
            await _context.ProjectMember.AddAsync(projectMember);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ProjectMember projectMember)
        {
            _context.ProjectMember.Update(projectMember);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ProjectMember projectMember)
        {
            _context.ProjectMember.Remove(projectMember);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ProjectMember>> GetProjectMemberbyProjectId(int projectId)
        {
            return await _context.ProjectMember
                .Where(tf => tf.ProjectId == projectId)
                .ToListAsync();
        }
        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            if (accountId <= 0) throw new ArgumentException("Invalid Account ID", nameof(accountId));
            return await _context.Account
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<List<ProjectMember>> GetByProjectIdAsync(int projectId)
        {
            return await _context.ProjectMember
                .Where(pm => pm.ProjectId == projectId)
                .ToListAsync();
        }

    }
}
