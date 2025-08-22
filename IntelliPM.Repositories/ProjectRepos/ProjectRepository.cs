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

        public async Task<List<ProjectManagerReportDto>> GetProjectManagerReportsAsync()
        {
            // Fetch project managers
            var projectManagers = await _context.Account
                .Join(_context.ProjectMember,
                    a => a.Id,
                    pm => pm.AccountId,
                    (a, pm) => new { Account = a, ProjectMember = pm })
                .Join(_context.ProjectPosition,
                    pm => pm.ProjectMember.Id,
                    pp => pp.ProjectMemberId,
                    (pm, pp) => new { pm.Account, pp })
                .Where(x => x.pp.Position == "PROJECT_MANAGER")
                .Select(x => new
                {
                    ProjectManagerId = x.Account.Id,
                    ProjectManagerName = x.Account.FullName ?? "Unknown"
                })
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            var result = new List<ProjectManagerReportDto>();

            foreach (var pm in projectManagers)
            {
                // Fetch projects for this project manager
                var projects = await _context.Project
                    .Join(_context.ProjectMember,
                        p => p.Id,
                        pmem => pmem.ProjectId,
                        (p, pmem) => new { Project = p, ProjectMember = pmem })
                    .Join(_context.ProjectPosition,
                        pm => pm.ProjectMember.Id,
                        pp => pp.ProjectMemberId,
                        (pm, pp) => new { pm.Project, pm.ProjectMember, pp })
                    .Where(x => x.pp.Position == "PROJECT_MANAGER" &&
                                x.ProjectMember.AccountId == pm.ProjectManagerId)
                    .GroupJoin(_context.ProjectMetric,
                        p => p.Project.Id,
                        pm => pm.ProjectId,
                        (p, pm) => new { p.Project, p.ProjectMember, p.pp, ProjectMetrics = pm })
                    .SelectMany(x => x.ProjectMetrics.DefaultIfEmpty(),
                        (p, pm) => new ProjectSummaryDto
                        {
                            ProjectId = p.Project.Id,
                            ProjectKey = p.Project.ProjectKey,
                            ProjectName = p.Project.Name,
                            Status = p.Project.Status ?? "UNKNOWN",
                            Spi = pm != null ? pm.SchedulePerformanceIndex : null,
                            Cpi = pm != null ? pm.CostPerformanceIndex : null,
                            Progress = _context.Tasks
                                .Where(t => t.ProjectId == p.Project.Id)
                                .Average(t => t.PercentComplete) ?? 0,
                            TotalTasks = _context.Tasks
                                .Count(t => t.ProjectId == p.Project.Id),
                            CompletedTasks = _context.Tasks
                                .Count(t => t.ProjectId == p.Project.Id && t.Status == "DONE"),
                            OverdueTasks = _context.Tasks
                                .Count(t => t.ProjectId == p.Project.Id &&
                                            t.Status != "DONE" &&
                                            t.PlannedEndDate < DateTime.UtcNow &&
                                            t.ActualEndDate == null),
                            Budget = p.Project.Budget ?? 0,
                            ActualCost = pm != null ? pm.ActualCost ?? 0 : 0,
                            RemainingBudget = p.Project.Budget != null && pm != null
                                ? (p.Project.Budget ?? 0) - (pm.ActualCost ?? 0)
                                : 0
                        })
                    .AsNoTracking()
                    .ToListAsync();

                // Fetch milestones for all projects
                var projectIds = projects.Select(p => p.ProjectId).ToList();
                var milestones = projectIds.Any()
                    ? await _context.Milestone
                        .Where(m => projectIds.Contains(m.ProjectId))
                        .Select(m => new MilestoneSummaryDto
                        {
                            MilestoneId = m.Id,
                            Key = m.Key,
                            Name = m.Name,
                            Status = m.Status ?? "UNKNOWN",
                            StartDate = m.StartDate,
                            EndDate = m.EndDate
                        })
                        .AsNoTracking()
                        .ToListAsync()
                    : new List<MilestoneSummaryDto>();

                // Assign milestones to projects
                foreach (var project in projects)
                {
                    project.Milestones = milestones
                        .Where(m => m.ProjectId == project.ProjectId)
                        .ToList();
                }

                var report = new ProjectManagerReportDto
                {
                    ProjectManagerId = pm.ProjectManagerId,
                    ProjectManagerName = pm.ProjectManagerName,
                    TotalProjects = projects.Count,
                    ActiveProjects = projects.Count(p => p.Status == "ACTIVE"),
                    OverdueTasks = projects.Sum(p => p.OverdueTasks),
                    TotalBudget = projects.Sum(p => p.Budget),
                    Projects = projects
                };

                result.Add(report);
            }

            return result;
        }
    }
}
