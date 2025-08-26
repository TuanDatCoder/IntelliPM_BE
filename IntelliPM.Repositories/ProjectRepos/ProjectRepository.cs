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
            // Lấy danh sách Project Manager
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
                // Lấy danh sách project cho từng PM
                var projects = await _context.Project
                    .Join(_context.ProjectMember,
                        p => p.Id,
                        pmem => pmem.ProjectId,
                        (p, pmem) => new { Project = p, ProjectMember = pmem })
                    .Join(_context.ProjectPosition,
                        pm2 => pm2.ProjectMember.Id,
                        pp => pp.ProjectMemberId,
                        (pm2, pp) => new { pm2.Project, pm2.ProjectMember, pp })
                    .Where(x => x.pp.Position == "PROJECT_MANAGER" &&
                                x.ProjectMember.AccountId == pm.ProjectManagerId)
                    .GroupJoin(_context.ProjectMetric,
                        p => p.Project.Id,
                        pm3 => pm3.ProjectId,
                        (p, pm3) => new { p.Project, p.ProjectMember, p.pp, ProjectMetrics = pm3 })
                    .SelectMany(x => x.ProjectMetrics.DefaultIfEmpty(),
                        (p, pmMetric) => new { p.Project, Metric = pmMetric })
                    .ToListAsync();

                var projectSummaries = new List<ProjectSummaryDto>();

                foreach (var p in projects)
                {
                    // Tính toán task-level metrics
                    var tasks = _context.Tasks.Where(t => t.ProjectId == p.Project.Id);

                    var totalTasks = tasks.Count();
                    var completedTasks = tasks.Count(t => t.Status == "DONE");
                    var overdueTasks = tasks.Count(t =>
                        t.Status != "DONE" &&
                        t.PlannedEndDate < DateTime.UtcNow &&
                        t.ActualEndDate == null);
                    var avgProgress = tasks.Any() ? tasks.Average(t => t.PercentComplete) ?? 0 : 0;

                    var totalActualCost = tasks.Sum(t => (t.ActualCost ?? 0) + (t.ActualResourceCost ?? 0));
                    var budget = p.Project.Budget ?? 0;

                    var summary = new ProjectSummaryDto
                    {
                        ProjectId = p.Project.Id,
                        ProjectKey = p.Project.ProjectKey,
                        ProjectName = p.Project.Name,
                        Status = p.Project.Status ?? "UNKNOWN",
                        Spi = p.Metric != null ? p.Metric.SchedulePerformanceIndex : null,
                        Cpi = p.Metric != null ? p.Metric.CostPerformanceIndex : null,
                        Progress = avgProgress,
                        TotalTasks = totalTasks,
                        CompletedTasks = completedTasks,
                        OverdueTasks = overdueTasks,
                        Budget = budget,
                        ActualCost = totalActualCost,
                        RemainingBudget = budget - totalActualCost
                    };

                    projectSummaries.Add(summary);
                }

                // Lấy milestone cho tất cả project
                var projectIds = projectSummaries.Select(p => p.ProjectId).ToList();
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
                            EndDate = m.EndDate,
                            ProjectId = m.ProjectId
                        })
                        .AsNoTracking()
                        .ToListAsync()
                    : new List<MilestoneSummaryDto>();

                // Gán milestone cho từng project
                foreach (var project in projectSummaries)
                {
                    project.Milestones = milestones
                        .Where(m => m.ProjectId == project.ProjectId)
                        .ToList();
                }

                // Tạo báo cáo cho PM
                var report = new ProjectManagerReportDto
                {
                    ProjectManagerId = pm.ProjectManagerId,
                    ProjectManagerName = pm.ProjectManagerName,
                    TotalProjects = projectSummaries.Count,
                    ActiveProjects = projectSummaries.Count(p => p.Status == "ACTIVE"),
                    OverdueTasks = projectSummaries.Sum(p => p.OverdueTasks),
                    TotalBudget = projectSummaries.Sum(p => p.Budget),
                    Projects = projectSummaries
                };

                result.Add(report);
            }

            return result;
        }

    }
}
