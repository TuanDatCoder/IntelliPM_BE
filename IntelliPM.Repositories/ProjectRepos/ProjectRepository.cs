using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Admin;
using IntelliPM.Data.DTOs.Project.Response;
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
            return await _context.Project
                .Where(p => p.ProjectKey == projectKey)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }


        public async Task<Project> GetProjectByNameAsync(string projectName)
        {
            return await _context.Project.FirstOrDefaultAsync(p => p.Name == projectName)
                ?? null;
        }
        //


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

        public async Task<List<ProjectItemDTO>> GetProjectItemsAsync(int projectId)
        {
            var taskIdList = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => t.Id)
                .ToListAsync();

            var taskDtos = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new ProjectItemDTO
                {
                    Id = t.Id,
                    Name = t.Title,
                    Type = "Task"
                }).ToListAsync();


            var milestoneDtos = await _context.Milestone
                .Where(m => m.ProjectId == projectId)
                .Select(m => new ProjectItemDTO
                {
                    Id = m.Key,
                    Name = m.Name,
                    Type = "Milestone"
                }).ToListAsync();

            var subtaskDtos = await _context.Subtask
                .Where(s => taskIdList.Contains(s.TaskId))
                .Select(s => new ProjectItemDTO
                {
                    Id = s.Id,
                    Name = s.Title,
                    Type = "Subtask"
                }).ToListAsync();

            return taskDtos
                .Concat(milestoneDtos)
                .Concat(subtaskDtos)
                .ToList();
        }

        //public async Task<List<ProjectStatusReportDto>> GetAllProjectStatusReportsAsync()
        //{
        //    var reports = await _context.Project
        //        .Include(p => p.Sprint)
        //        .Include(p => p.Milestone)
        //        .Include(p => p.Tasks)
        //            .ThenInclude(t => t.Subtask)
        //        .Include(p => p.ProjectMember)
        //            .ThenInclude(pm => pm.Account)
        //        .Select(project => new ProjectStatusReportDto
        //        {
        //            ProjectId = project.Id,
        //            ProjectName = project.Name,
        //            ProjectKey = project.ProjectKey,
        //            ProjectManager = project.ProjectMember
        //                .Where(pm => pm.Account.Position == "PROJECT_MANAGER")
        //                .Select(pm => pm.Account.FullName)
        //                .FirstOrDefault(),

        //            Budget = project.Budget,
        //            ActualCost = project.Tasks.Sum(t => t.ActualCost) ?? 0,

        //            TotalTasks = project.Tasks.Count(),
        //            CompletedTasks = project.Tasks.Count(t => t.Status == "DONE"),
        //            Progress = project.Tasks.Average(t => t.PercentComplete ?? 0),

        //            SPI = CalculateSPI(project.Tasks),
        //            CPI = CalculateCPI(project.Tasks),

        //            OverdueTasks = project.Tasks.Count(t =>
        //                t.PlannedEndDate < DateTime.UtcNow &&
        //                (t.Status != "DONE")),

        //            Milestones = project.Milestone.Select(m => new MilestoneDto
        //            {
        //                Name = m.Name,
        //                Status = m.Status,
        //                StartDate = m.StartDate,
        //                EndDate = m.EndDate
        //            }).ToList()
        //        })
        //        .ToListAsync();

        //    return reports;
        //}

        public async Task<List<ProjectStatusReportDto>> GetAllProjectStatusReportsAsync()
        {
            var projects = await _context.Project
                .Include(p => p.Sprint)
                .Include(p => p.Milestone)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Subtask)
                .Include(p => p.ProjectMember)
                    .ThenInclude(pm => pm.Account)
                .Include(p => p.ProjectMetric)
                .ToListAsync();

            var reports = projects.Select(project =>
            {
                var systemMetric = project.ProjectMetric
                    .FirstOrDefault(m => m.CalculatedBy == "System");

                return new ProjectStatusReportDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectKey = project.ProjectKey,
                    ProjectManager = project.ProjectMember
                        .Where(pm => pm.Account.Position == "PROJECT_MANAGER")
                        .Select(pm => pm.Account.FullName)
                        .FirstOrDefault(),

                    Budget = project.Budget,
                    ActualCost = project.Tasks.Sum(t => t.ActualCost) ?? 0,

                    TotalTasks = project.Tasks.Count(),
                    CompletedTasks = project.Tasks.Count(t => t.Status == "DONE"),
                    Progress = project.Tasks.Any() ? project.Tasks.Average(t => t.PercentComplete ?? 0) : 0,

                    SPI = systemMetric?.SchedulePerformanceIndex ?? 0,
                    CPI = systemMetric?.CostPerformanceIndex ?? 0,

                    OverdueTasks = project.Tasks.Count(t =>
                        t.PlannedEndDate < DateTime.UtcNow &&
                        t.Status != "DONE"),

                    Milestones = project.Milestone.Select(m => new MilestoneDto
                    {
                        Name = m.Name,
                        Status = m.Status,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate
                    }).ToList()
                };
            }).ToList();


            return reports;
        }

        public async Task<List<string>> GetAllProjectKeysAsync()
        {
            return await _context.Project.Select(p => p.ProjectKey).ToListAsync();
        }
    }
}
