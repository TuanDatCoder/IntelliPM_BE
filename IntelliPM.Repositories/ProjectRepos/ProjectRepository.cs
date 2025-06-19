using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.ProjectRepos
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public ProjectRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjects()
        {
            return await _context.Project
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Project
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Project>> SearchProjects(string searchTerm, string? projectType, string? status)
        {
            var query = _context.Project.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(projectType))
            {
                query = query.Where(p => p.ProjectType == projectType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.OrderBy(p => p.Id).ToListAsync();
        }

        public async Task Add(Project project)
        {
            await _context.Project.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Project project)
        {
            _context.Project.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Project project)
        {
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetProjectKeyAsync(int projectId)
        {
            var project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
            return project?.ProjectKey ?? string.Empty; 
        }

        public async Task<Project> GetProjectByKeyAsync(string projectKey)
        {
            return await _context.Project.FirstOrDefaultAsync(p => p.ProjectKey == projectKey)
                ?? null;
        }


        public async Task<Project> GetProjectWithMembersAndRequirements(int projectId)
        {
            return await _context.Project
                .Include(p => p.Requirement)
                .Include(p => p.ProjectMember)
                    .ThenInclude(pm => pm.ProjectPosition)
                    .Include(p => p.ProjectMember) 
                    .ThenInclude(pm => pm.Account) 
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }
    }
}
