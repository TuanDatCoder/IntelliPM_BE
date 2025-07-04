﻿using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.Entities;
using EntityTask = IntelliPM.Data.Entities.Tasks;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.TaskRepos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRecommendationRepos;
using IntelliPM.Repositories.DynamicCategoryRepos;

namespace IntelliPM.Services.ProjectMetricServices
{
    public class ProjectMetricService : IProjectMetricService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMetricRepository _repo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectRecommendationRepository _projectRecommendationRepo;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly ILogger<ProjectMetricService> _logger;
        private readonly IGeminiService _geminiService;

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, ITaskAssignmentRepository taskAssignmentRepo, IProjectMemberRepository projectMemberRepo, IProjectRecommendationRepository projectRecommendationRepo, IDynamicCategoryRepository dynamicCategoryRepo)
        {
            _mapper = mapper;
            _repo = repo;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _sprintRepo = sprintRepo;
            _milestoneRepo = milestoneRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _projectMemberRepo = projectMemberRepo;
            _projectRecommendationRepo = projectRecommendationRepo;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _logger = logger;
            _geminiService = geminiService;
        }

        public async Task<ProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(int projectId, string calculatedBy)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var today = DateTime.UtcNow;

            // Planned Value (PV): Tổng PlannedCost của các task kết thúc đến thời điểm hiện tại
            var plannedTasks = tasks.Where(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value <= today);
            decimal plannedValue = plannedTasks.Sum(t => t.PlannedCost ?? 0);

            // Earned Value (EV): Tổng (PlannedCost * % hoàn thành)
            decimal earnedValue = tasks.Sum(t => (t.PlannedCost ?? 0) * (decimal)((t.PercentComplete ?? 0) / 100));

            // Actual Cost (AC): Tổng ActualCost
            decimal actualCost = tasks.Sum(t => t.ActualCost ?? 0);

            // SPI = EV / PV
            double? spi = plannedValue == 0 ? null : (double?)Math.Round((double)earnedValue / (double)plannedValue, 2);

            // CPI = EV / AC
            double? cpi = actualCost == 0 ? null : (double?)Math.Round((double)earnedValue / (double)actualCost, 2);

            // Delay Days = số ngày trễ giữa ngày kết thúc dự kiến và ngày thực tế kết thúc trễ nhất
            var latestPlannedEnd = tasks.Max(t => t.PlannedEndDate);
            var latestActualEnd = tasks.Max(t => t.ActualEndDate);
            int? delayDays = (latestActualEnd.HasValue && latestPlannedEnd.HasValue)
                ? (int?)(latestActualEnd.Value - latestPlannedEnd.Value).TotalDays
                : null;

            // BudgetOverrun = AC - PV
            decimal? budgetOverrun = actualCost - plannedValue;

            // ProjectedFinishDate = lấy ngày kết thúc thực tế trễ nhất hoặc ngày dự kiến trễ nhất nếu chưa hoàn thành
            DateTime? projectedFinish = tasks.Any(t => t.ActualEndDate.HasValue)
                ? tasks.Max(t => t.ActualEndDate)
                : tasks.Max(t => t.PlannedEndDate);

            // Tổng chi phí ước lượng của toàn bộ dự án
            decimal totalCost = tasks.Sum(t => t.PlannedCost ?? 0);

            var metric = new ProjectMetric
            {
                ProjectId = projectId,
                CalculatedBy = calculatedBy,
                IsApproved = false,
                PlannedValue = plannedValue,
                EarnedValue = earnedValue,
                ActualCost = actualCost,
                Spi = (decimal?)spi,
                Cpi = (decimal?)cpi,
                DelayDays = delayDays,
                BudgetOverrun = budgetOverrun,
                ProjectedFinishDate = projectedFinish,
                ProjectedTotalCost = totalCost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.Add(metric);

            return _mapper.Map<ProjectMetricResponseDTO>(metric);
        }

        public async Task<ProjectMetricRequestDTO> CalculateProjectMetricsByAIAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            var result = await _geminiService.CalculateProjectMetricsAsync(project, tasks);
            if (result == null)
                throw new Exception("AI did not return valid project metrics");

            // Lưu ProjectMetric
            var existing = await _repo.GetByProjectIdAsync(project.Id);

            if (existing != null)
            {
                _mapper.Map(result, existing);
                existing.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(existing);
            }
            else
            {
                var metric = _mapper.Map<ProjectMetric>(result);
                metric.ProjectId = project.Id;
                metric.CreatedAt = DateTime.UtcNow;
                metric.UpdatedAt = DateTime.UtcNow;

                await _repo.Add(metric);
            }

            // Lưu các gợi ý nếu có
            if (result.Suggestions != null && result.Suggestions.Any())
            {
                var allTasks = tasks.ToDictionary(t => t.Title?.Trim(), t => t.Id);

                foreach (var suggestion in result.Suggestions)
                {
                    foreach (var related in suggestion.RelatedTasks)
                    {
                        var taskTitle = related.TaskTitle?.Trim();
                        if (string.IsNullOrEmpty(taskTitle) || !allTasks.ContainsKey(taskTitle))
                            continue;

                        var taskId = allTasks[taskTitle].ToString();
                        var type = suggestion.Label ?? suggestion.Reason ?? "AI";
                        var recommendationText = related.SuggestedAction ?? suggestion.Message;

                        var existingRec = await _projectRecommendationRepo.GetByProjectIdTaskIdTypeAsync(project.Id, taskId, type);

                        if (existingRec != null)
                        {
                            existingRec.Recommendation = recommendationText;
                            existingRec.CreatedAt = DateTime.UtcNow;

                            await _projectRecommendationRepo.Update(existingRec);
                        }
                        else
                        {
                            var rec = new ProjectRecommendation
                            {
                                ProjectId = project.Id,
                                TaskId = taskId,
                                Type = type,
                                Recommendation = recommendationText,
                                CreatedAt = DateTime.UtcNow,
                            };

                            await _projectRecommendationRepo.Add(rec);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<ProjectMetricRequestDTO> CalculateMetricsByAIAsync(int projectId)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null || tasks == null || !tasks.Any())
                throw new Exception("Không đủ dữ liệu để tính toán");

            // Gọi GeminiService để tính toán
            var result = await _geminiService.CalculateProjectMetricsAsync(project, tasks);

            result.ProjectId = projectId;
            result.CalculatedBy = "AI";

            return result;
        }

        public async Task<List<ProjectMetricResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<ProjectMetricResponseDTO>>(entities);
        }

        public async Task<ProjectMetricResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project metric with ID {id} not found.");

            return _mapper.Map<ProjectMetricResponseDTO>(entity);
        }

        public async Task<ProjectMetricResponseDTO?> GetByProjectIdAsync(int projectId)
        {
            var entity = await _repo.GetLatestByProjectIdAsync(projectId);
            return entity != null ? _mapper.Map<ProjectMetricResponseDTO>(entity) : null;
        }

        public async Task<CostDashboardResponseDTO> GetCostDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var taskAssignments = await _taskAssignmentRepo.GetByProjectIdAsync(project.Id);
            var projectMembers = await _projectMemberRepo.GetByProjectIdAsync(project.Id);

            // Tính Task Cost
            decimal actualTaskCost = tasks.Sum(t => t.ActualCost ?? 0);
            decimal plannedTaskCost = tasks.Sum(t => t.PlannedCost ?? 0);

            // Tính Resource Cost từ TaskAssignment và ProjectMember
            decimal actualResourceCost = taskAssignments.Sum(a =>
            {
                var hourly = projectMembers
                    .FirstOrDefault(m => m.ProjectId == project.Id && m.AccountId == a.AccountId)?.HourlyRate ?? 0;
                return (decimal)(a.ActualHours ?? 0) * hourly;
            });

            decimal plannedResourceCost = taskAssignments.Sum(a =>
            {
                var hourly = projectMembers
                    .FirstOrDefault(m => m.ProjectId == project.Id && m.AccountId == a.AccountId)?.HourlyRate ?? 0;
                return (decimal)(a.PlannedHours ?? 0) * hourly;
            });

            return new CostDashboardResponseDTO
            {
                ActualTaskCost = actualTaskCost,
                PlannedTaskCost = plannedTaskCost,
                ActualResourceCost = actualResourceCost,
                PlannedResourceCost = plannedResourceCost,

                ActualCost = actualTaskCost + actualResourceCost,
                PlannedCost = plannedTaskCost + plannedResourceCost,
                Budget = project.Budget ?? 0
            };
        }

        public async Task<List<object>> GetProgressDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);

            var result = new List<object>();

            foreach (var sprint in sprints)
            {
                var sprintTasks = tasks.Where(t => t.SprintId == sprint.Id).ToList();
                var sprintMilestones = milestones.Where(m => m.SprintId == sprint.Id).ToList(); 

                //var totalItems = sprintTasks.Count + sprintMilestones.Count;
                var totalItems = sprintTasks.Count;
                if (totalItems == 0)
                {
                    result.Add(new
                    {
                        sprintId = sprint.Id,
                        sprintName = sprint.Name,
                        percentComplete = 0.0
                    });
                    continue;
                }

                double taskProgress = (double)sprintTasks.Sum(t => t.PercentComplete ?? 0);
                //double milestoneProgress = sprintMilestones.Sum(m => m.PercentComplete ?? 0);

                //double percentComplete = (taskProgress + milestoneProgress) / totalItems;
                double percentComplete = taskProgress / totalItems;

                result.Add(new
                {
                    sprintId = sprint.Id,
                    sprintName = sprint.Name,
                    percentComplete = Math.Round(percentComplete, 2)
                });
            }

            return result;
        }

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var latestMetric = await _repo.GetLatestByProjectIdAsync(project.Id);

            double plannedDuration = tasks
                .Where(t => t.PlannedStartDate.HasValue && t.PlannedEndDate.HasValue)
                .Sum(t => (t.PlannedEndDate.Value - t.PlannedStartDate.Value).TotalDays);

            double actualDuration = tasks
                .Where(t => t.ActualStartDate.HasValue && t.ActualEndDate.HasValue)
                .Sum(t => (t.ActualEndDate.Value - t.ActualStartDate.Value).TotalDays);

            string timeStatus = "On track";
            if (plannedDuration > 0)
            {
                var deviation = ((actualDuration - plannedDuration) / plannedDuration) * 100;
                timeStatus = deviation >= 0 ? $"{Math.Round(deviation, 2)}% behind" : $"{Math.Abs(Math.Round(deviation, 2))}% ahead";
            }

            int tasksToBeCompleted = tasks.Count(t => t.Status != "CM");
            int overdueTasks = tasks.Count(t => t.PlannedEndDate < DateTime.UtcNow && t.Status != "CM");
            double progress = tasks.Any()
                ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
                : 0;

            decimal costStatus = 0;
            var costDto = new ProjectMetricResponseDTO();
            if (latestMetric != null)
            {
                if (latestMetric.ActualCost.HasValue && latestMetric.ActualCost != 0)
                    costStatus = Math.Round(latestMetric.EarnedValue.GetValueOrDefault() / latestMetric.ActualCost.Value, 2);
                costDto = _mapper.Map<ProjectMetricResponseDTO>(latestMetric);
            }

            return new ProjectHealthDTO
            {
                TimeStatus = timeStatus,
                TasksToBeCompleted = tasksToBeCompleted,
                OverdueTasks = overdueTasks,
                ProgressPercent = Math.Round(progress, 2),
                CostStatus = costStatus,
                Cost = costDto
            };
        }

        public async Task<object> GetTaskStatusDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var statusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "task_status");

            //var notStarted = tasks.Count(t => string.Equals(t.Status, "NOT_STARTED", StringComparison.OrdinalIgnoreCase));
            //var inProgress = tasks.Count(t => string.Equals(t.Status, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase));
            //var completed = tasks.Count(t => string.Equals(t.Status, "COMPLETE", StringComparison.OrdinalIgnoreCase));

            //return new
            //{
            //    notStarted,
            //    inProgress,
            //    completed
            //};

            var statusCounts = statusCategories
                .Select(c => new
                {
                    key = c.Id,
                    name = c.Name,
                    count = tasks.Count(t => string.Equals(t.Status, c.Name, StringComparison.OrdinalIgnoreCase))
                })
                .ToList();

            return new
            {
                statusCounts,
            };
        }

        public async Task<object> GetTimeDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id)
                ?? throw new Exception("Project has no tasks");

            var today = DateTime.UtcNow;

            decimal totalPlannedCost = tasks.Sum(t => t.PlannedCost ?? 0);
            decimal plannedCostTillToday = tasks
                .Where(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value <= today)
                .Sum(t => t.PlannedCost ?? 0);

            // Nếu không có PlannedCost, fallback sang số lượng task
            double plannedCompletion;
            if (totalPlannedCost > 0)
            {
                plannedCompletion = (double)(plannedCostTillToday / totalPlannedCost) * 100;
            }
            else
            {
                int totalTasks = tasks.Count;
                int expectedCompletedTasks = tasks.Count(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value <= today);
                plannedCompletion = totalTasks == 0 ? 0 : (double)expectedCompletedTasks / totalTasks * 100;
            }

            double actualCompletion = (double)tasks.Average(t => t.PercentComplete ?? 0);

            string status;
            double diff = actualCompletion - plannedCompletion;

            if (diff > 5)
                status = "Ahead";
            else if (diff < -5)
                status = "Behind";
            else
                status = "On Time";

            return new
            {
                plannedCompletion = Math.Round(plannedCompletion, 2),
                actualCompletion = Math.Round(actualCompletion, 2),
                status
            };
        }

        public async Task<List<WorkloadDashboardResponseDTO>> GetWorkloadDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var projectMembers = await _projectMemberRepo.GetByProjectIdAsync(project.Id);
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var taskAssignments = await _taskAssignmentRepo.GetByProjectIdAsync(project.Id);

            var today = DateTime.Today;

            var result = projectMembers.Select(member =>
            {
                var assignedTaskIds = taskAssignments
                    .Where(a => a.AccountId == member.AccountId)
                    .Select(a => a.TaskId)
                    .Distinct()
                    .ToList();

                var memberTasks = tasks.Where(t => assignedTaskIds.Contains(t.Id)).ToList();

                var completed = memberTasks.Count(t => t.PercentComplete == 100);
                var overdue = memberTasks.Count(t =>
                    t.PercentComplete < 100 &&
                    t.PlannedEndDate.HasValue &&
                    t.PlannedEndDate.Value.Date < today
                );

                var remaining = memberTasks.Count(t =>
                    t.PercentComplete < 100 &&
                    (!t.PlannedEndDate.HasValue || t.PlannedEndDate.Value.Date >= today)
                );

                return new WorkloadDashboardResponseDTO
                {
                    MemberName = member.Account?.FullName ?? "Unknown",
                    Completed = completed,
                    Remaining = remaining,
                    Overdue = overdue
                };
            }).ToList();

            return result;
        }
    }
}
