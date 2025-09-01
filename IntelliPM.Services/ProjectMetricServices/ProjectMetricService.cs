using AutoMapper;
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
using Google.Cloud.Storage.V1;
using IntelliPM.Services.ChatGPTServices;
using CloudinaryDotNet;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.MetricHistoryRepos;
using System.Text.Json;
using IntelliPM.Repositories.SystemConfigurationRepos;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Data.Enum.Task;
using IntelliPM.Data.Enum.ProjectMetric;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Data.Enum.Account;

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
        private readonly IChatGPTService _chatGPTService;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IMetricHistoryRepository _metricHistoryRepo;
        private readonly ISystemConfigurationRepository _systemConfigRepo;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;
        private readonly IProjectPositionRepository _projectPositionRepo;

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, ITaskAssignmentRepository taskAssignmentRepo, IProjectMemberRepository projectMemberRepo, IProjectRecommendationRepository projectRecommendationRepo, IDynamicCategoryRepository dynamicCategoryRepo, IChatGPTService chatGPTService, ISubtaskRepository subtaskRepo, IMetricHistoryRepository metricHistoryRepo, ISystemConfigurationRepository systemConfigurationRepo, IDynamicCategoryHelper dynamicCategoryHelper, IProjectPositionRepository projectPositionRepo)
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
            _subtaskRepo = subtaskRepo;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _logger = logger;
            _geminiService = geminiService;
            _chatGPTService = chatGPTService;
            _metricHistoryRepo = metricHistoryRepo;
            _systemConfigRepo = systemConfigurationRepo;
            _dynamicCategoryHelper = dynamicCategoryHelper;
            _projectPositionRepo = projectPositionRepo;
        }

        public async Task<NewProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(string projectKey) //
        {
            // Fetch project and tasks
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            if (tasks == null || !tasks.Any())
                throw new InvalidOperationException("No tasks found for the project.");

            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");

            // Fetch configurations
            var statusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "project_status");

            decimal PV = 0; // Planned Value
            decimal EV = 0; // Earned Value
            decimal AC = 0; // Actual Cost
            decimal BAC = project.Budget ?? 0; // Budget At Completion
            decimal DAC = 0; // Duration At Completion (months)

            // Check if project has started
            var now = DateTime.UtcNow;
            bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;

            // Calculate Duration At Completion
            if (project.StartDate.HasValue && project.EndDate.HasValue)
            {
                var totalDays = (project.EndDate.Value - project.StartDate.Value).TotalDays;
                DAC = Math.Round((decimal)(totalDays / 30.0), 0);
            }

            // Track progress
            bool hasProgress = false;
            
            foreach (var task in tasks)
            {
                // Planned cost (bao gồm cả resource)
                var taskPlannedCost = (task.PlannedCost ?? 0) + (task.PlannedResourceCost ?? 0);
                // Actual cost (bao gồm cả resource)
                var taskActualCost = (task.ActualCost ?? 0) + (task.ActualResourceCost ?? 0);
                // Calculate PV: Based on planned cost and current date within planned range
                if (hasProjectStarted && task.PlannedStartDate.HasValue && task.PlannedEndDate.HasValue)
                {
                    var taskStart = task.PlannedStartDate.Value.Date;
                    var taskEnd = task.PlannedEndDate.Value.Date;
                    if (now.Date >= taskStart && now.Date <= taskEnd)
                    {
                        var totalPlannedDuration = (taskEnd - taskStart).TotalDays;
                        totalPlannedDuration = totalPlannedDuration == 0 ? 1 : totalPlannedDuration;
                        var elapsed = (now - taskStart).TotalDays;
                        var progress = (decimal)(elapsed / totalPlannedDuration);
                        //PV += (task.PlannedCost ?? 0) * progress;
                        PV += taskPlannedCost * progress;
                    }
                    else if (now.Date > taskEnd)
                    {
                        //PV += (task.PlannedCost ?? 0);
                        PV += taskPlannedCost;
                    }
                }

                // Calculate EV: Based on percentage complete * planned cost
                if (task.PercentComplete.HasValue)
                {
                    EV += taskPlannedCost * (task.PercentComplete.Value / 100);
                    if (task.PercentComplete.Value > 0)
                        hasProgress = true;
                }
                //if (task.PercentComplete.HasValue && task.PlannedCost.HasValue)
                //{
                //    EV += task.PlannedCost.Value * (task.PercentComplete.Value / 100);
                //    if (task.PercentComplete.Value > 0)
                //        hasProgress = true;
                //}

                //Calculate AC
                //AC += (task.ActualCost ?? 0);
                AC += taskActualCost;
            } 

            // Calculate performance metrics
            decimal CV = EV - AC;
            decimal SV = EV - PV;
            //decimal CPI = hasProjectStarted && AC > 0 ? EV / AC : 0;
            //decimal SPI = hasProjectStarted && PV > 0 ? EV / PV : 0;
            decimal CPI = AC == 0 ? 0 : EV / AC;
            decimal SPI = PV == 0 ? 0 : EV / PV;
            decimal EAC = CPI > 0 ? BAC / CPI : BAC; // Fallback to BAC if CPI is 0
            decimal ETC = EAC - AC;
            decimal VAC = BAC - EAC;
            decimal EDAC = SPI > 0 ? DAC / SPI : DAC; // Fallback to DAC if SPI is 0
            // Save to project_metric_history for trend analysis
            var metricHistory = new ProjectMetricHistory
            {
                ProjectId = project.Id,
                MetricKey = "SYSTEM_CALCULATION",
                Value = JsonConvert.SerializeObject(new { PV, EV, AC, CPI, SPI, EAC, ETC, VAC, EDAC }),
                RecordedAt = DateTime.UtcNow
            };
            try
            {
                await _metricHistoryRepo.Add(metricHistory);

                var existingMetric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);

                if (existingMetric != null)
                {
                    existingMetric.PlannedValue = PV;
                    existingMetric.EarnedValue = EV;
                    existingMetric.ActualCost = AC;
                    existingMetric.BudgetAtCompletion = BAC;
                    existingMetric.DurationAtCompletion = DAC;
                    existingMetric.CostVariance = CV;
                    existingMetric.ScheduleVariance = SV;
                    existingMetric.CostPerformanceIndex = Math.Round(CPI, 3);
                    existingMetric.SchedulePerformanceIndex = Math.Round(SPI, 3);
                    existingMetric.EstimateAtCompletion = Math.Round(EAC, 0);
                    existingMetric.EstimateToComplete = Math.Round(ETC, 0);
                    existingMetric.VarianceAtCompletion = Math.Round(VAC, 0);
                    existingMetric.EstimateDurationAtCompletion = Math.Round(EDAC, 0);
                    existingMetric.UpdatedAt = DateTime.UtcNow;

                    await _repo.Update(existingMetric);
                    var result = _mapper.Map<NewProjectMetricResponseDTO>(existingMetric);
                    return result;
                }
                else
                {
                    var newMetric = new ProjectMetric
                    {
                        ProjectId = project.Id,
                        PlannedValue = PV,
                        EarnedValue = EV,
                        ActualCost = AC,
                        BudgetAtCompletion = BAC,
                        DurationAtCompletion = DAC,
                        CostVariance = CV,
                        ScheduleVariance = SV,
                        CostPerformanceIndex = Math.Round(CPI, 3),
                        SchedulePerformanceIndex = Math.Round(SPI, 3),
                        EstimateAtCompletion = Math.Round(EAC, 0),
                        EstimateToComplete = Math.Round(ETC, 0),
                        VarianceAtCompletion = Math.Round(VAC, 0),
                        EstimateDurationAtCompletion = Math.Round(EDAC, 0),
                        CalculatedBy = calculationMode,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    await _repo.Add(newMetric);
                    var result = _mapper.Map<NewProjectMetricResponseDTO>(newMetric);
                    return result;
                }
            } catch (Exception ex)
            {
                // Log ex.InnerException.Message and ex.InnerException.StackTrace
                Console.WriteLine($"Error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<ProjectMetricRequestDTO> CalculateMetricsByAIAsync(int projectId)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null || tasks == null || !tasks.Any())
                throw new Exception("Không đủ dữ liệu để tính toán");

            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "AI");
            // Gọi GeminiService để tính toán
            var result = await _geminiService.CalculateProjectMetricsAsync(project, tasks);

            result.ProjectId = projectId;
            result.CalculatedBy = calculationMode;

            return result;
        }

        public async Task<List<NewProjectMetricResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<NewProjectMetricResponseDTO>>(entities);
        }

        public async Task<NewProjectMetricResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project metric with ID {id} not found.");

            return _mapper.Map<NewProjectMetricResponseDTO>(entity);
        }

        public async Task<NewProjectMetricResponseDTO?> GetByProjectIdAsync(int projectId)
        {
            var entity = await _repo.GetLatestByProjectIdAsync(projectId);
            return entity != null ? _mapper.Map<NewProjectMetricResponseDTO>(entity) : null;
        }

        //public async Task<List<object>> GetProgressDashboardAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");
        //    var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

        //    var result = new List<object>();

        //    foreach (var sprint in sprints)
        //    {
        //        var sprintTasks = tasks.Where(t => t.SprintId == sprint.Id).ToList();

        //        if (!sprintTasks.Any())
        //        {
        //            result.Add(new
        //            {
        //                sprintId = sprint.Id,
        //                sprintName = sprint.Name,
        //                percentComplete = 0.0
        //            });
        //            continue;
        //        }

        //        // Weighted calculation
        //        decimal totalPlanned = sprintTasks.Sum(t => t.PlannedHours ?? 0);
        //        if (totalPlanned == 0)
        //        {
        //            // Nếu không có planned hours thì fallback về average % complete
        //            decimal avgPercent = sprintTasks.Average(t => t.PercentComplete ?? 0);
        //            result.Add(new
        //            {
        //                sprintId = sprint.Id,
        //                sprintName = sprint.Name,
        //                percentComplete = Math.Round(avgPercent, 2)
        //            });
        //        }
        //        else
        //        {
        //            decimal weightedProgress = sprintTasks.Sum(t =>
        //                (t.PlannedHours ?? 0) * (t.PercentComplete ?? 0));
        //            decimal percentComplete = weightedProgress / totalPlanned;

        //            result.Add(new
        //            {
        //                sprintId = sprint.Id,
        //                sprintName = sprint.Name,
        //                percentComplete = Math.Round(percentComplete, 2)
        //            });
        //        }
        //    }

        //    return result;
        //}

        public async Task<List<object>> GetProgressDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            // Log task and sprint counts
            Console.WriteLine($"Total tasks: {tasks.Count}");
            Console.WriteLine($"Total sprints: {sprints.Count}");
            Console.WriteLine($"Tasks with SprintId == null: {tasks.Count(t => t.SprintId == null)}");

            var result = new List<object>();

            foreach (var sprint in sprints)
            {
                var sprintTasks = tasks.Where(t => t.SprintId == sprint.Id).ToList();

                // Log tasks per sprint
                Console.WriteLine($"Sprint {sprint.Name} (ID: {sprint.Id}): {sprintTasks.Count} tasks");

                if (!sprintTasks.Any())
                {
                    result.Add(new
                    {
                        sprintId = sprint.Id,
                        sprintName = sprint.Name,
                        percentComplete = 0.0
                    });
                    continue;
                }

                // Calculate BAC and EV for the sprint
                decimal bac = sprintTasks.Sum(t => (t.PlannedCost ?? 0) + (t.PlannedResourceCost ?? 0));
                decimal ev = sprintTasks.Sum(t => ((t.PlannedCost ?? 0) + (t.PlannedResourceCost ?? 0)) * ((t.PercentComplete ?? 0) / 100m));
                decimal percentComplete;

                if (bac > 0)
                {
                    percentComplete = (ev / bac) * 100m;
                }
                else
                {
                    decimal totalPlanned = sprintTasks.Sum(t => t.PlannedHours ?? 0);
                    if (totalPlanned > 0)
                    {
                        decimal weightedProgress = sprintTasks.Sum(t =>
                            (t.PlannedHours ?? 0) * (t.PercentComplete ?? 0));
                        percentComplete = weightedProgress / totalPlanned;
                    }
                    else
                    {
                        percentComplete = sprintTasks.Average(t => t.PercentComplete ?? 0);
                    }
                }

                result.Add(new
                {
                    sprintId = sprint.Id,
                    sprintName = sprint.Name,
                    percentComplete = Math.Round(percentComplete, 2)
                });
            }

            // Handle tasks with no sprint (SprintId is null)
            var noSprintTasks = tasks.Where(t => t.SprintId == null).ToList();
            Console.WriteLine($"No Sprint tasks: {noSprintTasks.Count}");

            if (noSprintTasks.Any())
            {
                decimal bac = noSprintTasks.Sum(t => (t.PlannedCost ?? 0) + (t.PlannedResourceCost ?? 0));
                decimal ev = noSprintTasks.Sum(t => ((t.PlannedCost ?? 0) + (t.PlannedResourceCost ?? 0)) * ((t.PercentComplete ?? 0) / 100m));
                decimal percentComplete;

                if (bac > 0)
                {
                    percentComplete = (ev / bac) * 100m;
                }
                else
                {
                    decimal totalPlanned = noSprintTasks.Sum(t => t.PlannedHours ?? 0);
                    if (totalPlanned > 0)
                    {
                        decimal weightedProgress = noSprintTasks.Sum(t =>
                            (t.PlannedHours ?? 0) * (t.PercentComplete ?? 0));
                        percentComplete = weightedProgress / totalPlanned;
                    }
                    else
                    {
                        percentComplete = noSprintTasks.Average(t => t.PercentComplete ?? 0);
                    }
                }

                result.Add(new
                {
                    sprintId = (int?)null,
                    sprintName = "No Sprint",
                    percentComplete = Math.Round(percentComplete, 2)
                });
            }

            return result;
        }

        public async Task<object> GetTaskStatusDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var statusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "task_status");

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
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);
            var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");

            var spiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("spi_warning_threshold");

            decimal spiWarningThreshold = spiWarningThresholdConfig != null ? decimal.Parse(spiWarningThresholdConfig.ValueConfig) : 1m;

            decimal spi = metric.SchedulePerformanceIndex ?? 0;
            string status;
            if (spi < spiWarningThreshold)
                status = healthStatusCategories.FirstOrDefault(c => c.Name == ScheduleStatusEnum.BEHIND.ToString())?.Label;
            else if (spi > spiWarningThreshold)
                status = healthStatusCategories.FirstOrDefault(c => c.Name == ScheduleStatusEnum.AHEAD.ToString())?.Label;
            else
                status = healthStatusCategories.FirstOrDefault(c => c.Name == ScheduleStatusEnum.ON_TIME.ToString())?.Label;

            return new
            {
                plannedCompletion = Math.Round((double)(metric.PlannedValue / metric.BudgetAtCompletion) * 100, 2),
                actualCompletion = Math.Round((double)(metric.EarnedValue / metric.BudgetAtCompletion) * 100, 2),
                status
            };
        }


        public async Task<List<WorkloadDashboardResponseDTO>> GetWorkloadDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var projectMembers = await _projectMemberRepo.GetByProjectIdAsync(project.Id);
            var projectPositions = await _projectPositionRepo.GetByProjectIdAsync(project.Id);
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var taskAssignments = await _taskAssignmentRepo.GetByProjectIdAsync(project.Id);

            var filteredMembers = projectMembers
                .Where(pm => !projectPositions
                .Any(pp => pp.ProjectMemberId == pm.Id && pp.Position == AccountPositionEnum.CLIENT.ToString()))
            .ToList();

            var today = DateTime.Today;

            var result = filteredMembers.Select(member =>
            {
                var assignedTaskIds = taskAssignments
                    .Where(a => a.AccountId == member.AccountId)
                    .Select(a => a.TaskId)
                    .Distinct()
                    .ToList();

                var memberTasks = tasks
                    .Where(t => assignedTaskIds.Contains(t.Id))
                    .ToList();

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
                    Overdue = overdue,
                    Remaining = remaining
                };
            }).ToList();

            return result;
        }

        public async Task<NewProjectMetricResponseDTO?> GetByProjectKeyAsync(string projectKey) 
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var entity = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);

            return _mapper.Map<NewProjectMetricResponseDTO>(entity);
        }

        public async Task<NewProjectMetricResponseDTO> GetProjectForecastByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "AI");
            var entity = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);

            return _mapper.Map<NewProjectMetricResponseDTO>(entity);
        }

        public async Task<CostDashboardResponseDTO> GetCostDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            // Tính chi phí từ tasks table
            decimal actualTaskCost = tasks.Sum(t => (t.ActualCost ?? 0));
            decimal plannedTaskCost = tasks.Sum(t => (t.PlannedCost ?? 0));

            decimal actualResourceCost = tasks.Sum(t => (t.ActualResourceCost ?? 0));
            decimal plannedResourceCost = tasks.Sum(t => (t.PlannedResourceCost ?? 0));

            // Tổng chi phí
            decimal actualCost = actualTaskCost + actualResourceCost;
            decimal plannedCost = plannedTaskCost + plannedResourceCost;

            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);

            decimal earnedValue = metric?.EarnedValue ?? 0; // EV: giá trị thu được từ công việc đã hoàn thành

            return new CostDashboardResponseDTO
            {
                ActualCost = actualCost,
                ActualTaskCost = actualTaskCost,
                ActualResourceCost = actualResourceCost,
                PlannedCost = plannedCost,
                PlannedTaskCost = plannedTaskCost, 
                PlannedResourceCost = plannedResourceCost,
                EarnedValue = Math.Round(earnedValue, 0),
                Budget = project.Budget ?? 0
            };
        }

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);

            // Fetch configurations
            var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");
            var spiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("spi_warning_threshold");
            var cpiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("cpi_warning_threshold");
            var minDaysForAlertConfig = await _systemConfigRepo.GetByConfigKeyAsync("minimum_days_for_alert");
            var minProgressForAlertConfig = await _systemConfigRepo.GetByConfigKeyAsync("minimum_progress_for_alert");

            // Parse configuration values with defaults
            decimal spiWarningThreshold = spiWarningThresholdConfig != null ? decimal.Parse(spiWarningThresholdConfig.ValueConfig) : 1m;
            decimal cpiWarningThreshold = cpiWarningThresholdConfig != null ? decimal.Parse(cpiWarningThresholdConfig.ValueConfig) : 1m;
            int minDaysForAlert = minDaysForAlertConfig != null ? int.Parse(minDaysForAlertConfig.ValueConfig) : 7;
            double minProgressForAlert = minProgressForAlertConfig != null ? double.Parse(minProgressForAlertConfig.ValueConfig) : 5.0;

            // Calculate tasks metrics
            var doneStatus = (await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync(TaskStatusEnum.DONE.ToString(), "task_status"))?.FirstOrDefault()?.Name ?? TaskStatusEnum.DONE.ToString();
            int tasksToBeCompleted = tasks.Count(t => !string.Equals(t.Status, doneStatus, StringComparison.OrdinalIgnoreCase));
            int overdueTasks = tasks.Count(t =>
                t.PlannedEndDate.HasValue &&
                t.PlannedEndDate.Value < DateTime.UtcNow &&
                !string.Equals(t.Status, doneStatus, StringComparison.OrdinalIgnoreCase));

            //double progress = tasks.Any()
            //    ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
            //    : 0;

            double progress = 0;

            if (metric != null && metric.BudgetAtCompletion > 0)
            {
                // Progress theo EVM
                progress = (double)(metric.EarnedValue / metric.BudgetAtCompletion) * 100.0;
            }
            else if (tasks.Any())
            {
                // Fallback: Weighted theo PlannedHours (nếu BAC chưa có)
                double totalPlanned = tasks.Sum(t => (double)(t.PlannedHours ?? 0));
                if (totalPlanned > 0)
                {
                    progress = tasks.Sum(t => (double)(t.PlannedHours ?? 0) * ((double)(t.PercentComplete ?? 0) / 100.0))
                               / totalPlanned * 100.0;
                }
                else
                {
                    // Nếu không có PlannedHours thì lấy trung bình %Complete
                    progress = tasks.Average(t => (double)(t.PercentComplete ?? 0));
                }
            }

            // Use project.Status directly, but map to label
            var projectStatusCategory = (await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync(project.Status ?? "PLANNING", "project_status"))?.FirstOrDefault();
            string projectStatus = projectStatusCategory?.Label ?? project.Status ?? "Not Started";

            // Initialize statuses
            string behindTimeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, ScheduleStatusEnum.BEHIND.ToString(), StringComparison.OrdinalIgnoreCase))?.Label 
                ?? throw new Exception("BEHIND status label not found in health status categories");
            string onTimeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, ScheduleStatusEnum.ON_TIME.ToString(), StringComparison.OrdinalIgnoreCase))?.Label
            ?? throw new Exception("ON_TIME status label not found in health status categories");
            string aheadTimeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, ScheduleStatusEnum.AHEAD.ToString(), StringComparison.OrdinalIgnoreCase))?.Label
            ?? throw new Exception("AHEAD status label not found in health status categories");


            string underBudgetStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, CostStatusEnum.UNDER_BUDGET.ToString(), StringComparison.OrdinalIgnoreCase))?.Label
                ?? throw new Exception("UNDER_BUDGET status label not found in health status categories");
            string overBudgetStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, CostStatusEnum.OVER_BUDGET.ToString(), StringComparison.OrdinalIgnoreCase))?.Label 
                ?? throw new Exception("OVER_BUDGET status label not found in health status categories");
            string onBudgetStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, CostStatusEnum.ON_BUDGET.ToString(), StringComparison.OrdinalIgnoreCase))?.Label
                ?? throw new Exception("ON_BUDGET status label not found in health status categories");

            string timeStatus = behindTimeStatus; // Default to BEHIND status
            string costStatus = overBudgetStatus; // Default to OVER_BUDGET status

            var costDto = new NewProjectMetricResponseDTO();
            if (metric != null)
            {
                costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);
                costDto.CostPerformanceIndex = Math.Round(metric.CostPerformanceIndex ?? 0, 3);
                costDto.SchedulePerformanceIndex = Math.Round(metric.SchedulePerformanceIndex ?? 0, 3);
                costDto.EstimateDurationAtCompletion = Math.Round(metric.EstimateDurationAtCompletion ?? 0, 1);
                costDto.EstimateAtCompletion = Math.Round(metric.EstimateAtCompletion ?? 0, 0);
                costDto.EstimateToComplete = Math.Round(metric.EstimateToComplete ?? 0, 0);
                costDto.VarianceAtCompletion = Math.Round(metric.VarianceAtCompletion ?? 0, 0);
            }

            var now = DateTime.UtcNow;
            bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;
            bool hasProgress = tasks.Any(t => (t.PercentComplete ?? 0) > 0);
            bool isBeyondMinDays = project.StartDate.HasValue && (now - project.StartDate.Value).TotalDays >= minDaysForAlert;


            if (metric != null && hasProjectStarted)
            {
                if (metric.SchedulePerformanceIndex < spiWarningThreshold)
                {
                    timeStatus = $"{Math.Round((1 - (double)metric.SchedulePerformanceIndex) * 100, 2)}% {behindTimeStatus}";
                }
                else if (metric.SchedulePerformanceIndex > spiWarningThreshold)
                {
                    timeStatus = $"{Math.Round(((double)metric.SchedulePerformanceIndex - 1) * 100, 2)}% {aheadTimeStatus}";
                }
                else
                {
                    timeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, ScheduleStatusEnum.ON_TIME.ToString(), StringComparison.OrdinalIgnoreCase))?.Label ?? "On Time";
                }

                // Calculate costStatus based on CPI

                if (metric.CostPerformanceIndex < cpiWarningThreshold)
                {
                    costStatus = $"{Math.Round((1 - (double)metric.CostPerformanceIndex) * 100, 2)}% {overBudgetStatus}";
                }
                else if (metric.CostPerformanceIndex > cpiWarningThreshold)
                {
                    costStatus = $"{Math.Round(((double)metric.CostPerformanceIndex - 1) * 100, 2)}% {underBudgetStatus}";
                }
                else
                {
                    costStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, CostStatusEnum.ON_BUDGET.ToString(), StringComparison.OrdinalIgnoreCase))?.Label ?? "On Budget";
                }
            }

            bool showAlert = false;

            if (metric != null && hasProjectStarted && isBeyondMinDays && progress >= minProgressForAlert)
            {
                showAlert = costDto.SchedulePerformanceIndex < spiWarningThreshold || costDto.CostPerformanceIndex < cpiWarningThreshold;
            }

            return new ProjectHealthDTO
            {
                ProjectStatus = projectStatus,
                TimeStatus = timeStatus,
                TasksToBeCompleted = tasksToBeCompleted,
                OverdueTasks = overdueTasks,
                ProgressPercent = Math.Round(progress, 2),
                CostStatus = costStatus,
                Cost = costDto,
                ShowAlert = showAlert
            };
        }
    }
}
