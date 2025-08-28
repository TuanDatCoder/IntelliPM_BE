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

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, ITaskAssignmentRepository taskAssignmentRepo, IProjectMemberRepository projectMemberRepo, IProjectRecommendationRepository projectRecommendationRepo, IDynamicCategoryRepository dynamicCategoryRepo, IChatGPTService chatGPTService, ISubtaskRepository subtaskRepo, IMetricHistoryRepository metricHistoryRepo, ISystemConfigurationRepository systemConfigurationRepo)
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
        }

        //public async Task<NewProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");
        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

        //    if (tasks == null || !tasks.Any())
        //        throw new InvalidOperationException("No tasks found for the project.");

        //    decimal PV = 0; // Planned Value
        //    decimal EV = 0; // Earned Value
        //    decimal AC = 0; // Actual Cost
        //    decimal BAC = (decimal)project.Budget; // Budget At Completion
        //    decimal DAC = 0; // Duration At Completion (tháng)
        //    if (project.StartDate.HasValue && project.EndDate.HasValue)
        //    {
        //        var totalDays = (project.EndDate.Value - project.StartDate.Value).TotalDays;
        //        DAC = Math.Round((decimal)(totalDays / 30.0), 0); 
        //    }

        //    foreach (var task in tasks)
        //    {
        //        // Tính PV: dựa vào planned cost và ngày hiện tại trong planned range
        //        if (task.PlannedStartDate.HasValue && task.PlannedEndDate.HasValue)
        //        {
        //            var now = DateTime.UtcNow.Date; // Use date-only to ignore time component
        //            if (now >= task.PlannedStartDate.Value.Date && now <= task.PlannedEndDate.Value.Date)
        //            {
        //                var totalPlannedDuration = (task.PlannedEndDate - task.PlannedStartDate)?.TotalDays ?? 1;
        //                var elapsed = (now - task.PlannedStartDate)?.TotalDays ?? 0;
        //                var progress = (decimal)(elapsed / totalPlannedDuration);
        //                PV += (task.PlannedCost ?? 0) * progress;
        //            }
        //            else if (now > task.PlannedEndDate.Value.Date)
        //            {
        //                PV += task.PlannedCost ?? 0;
        //            }
        //        }

        //        // EV: dựa vào phần trăm hoàn thành * planned cost
        //        if (task.PercentComplete.HasValue && task.PlannedCost.HasValue)
        //        {
        //            EV += task.PlannedCost.Value * (task.PercentComplete.Value / 100);
        //        }

        //        // AC: Actual Cost
        //        AC += task.ActualCost ?? 0;
        //    }

        //    // Công thức hiệu suất chi phí và tiến độ
        //    decimal CV = EV - AC;
        //    decimal SV = EV - PV;
        //    decimal CPI = AC == 0 ? 0 : EV / AC;
        //    decimal SPI = PV == 0 ? 0 : EV / PV;
        //    decimal EAC = CPI == 0 ? 0 : BAC / CPI;
        //    decimal ETC = EAC - AC;
        //    decimal VAC = BAC - EAC;
        //    decimal EDAC = SPI == 0 ? DAC : DAC / SPI;

        //    var existingMetric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

        //    if (existingMetric != null)
        //    {
        //        existingMetric.PlannedValue = PV;
        //        existingMetric.EarnedValue = EV;
        //        existingMetric.ActualCost = AC;
        //        existingMetric.BudgetAtCompletion = BAC;
        //        existingMetric.DurationAtCompletion = DAC;
        //        existingMetric.CostVariance = CV;
        //        existingMetric.ScheduleVariance = SV;
        //        existingMetric.CostPerformanceIndex = Math.Round(CPI, 3);
        //        existingMetric.SchedulePerformanceIndex = Math.Round(SPI, 3);
        //        existingMetric.EstimateAtCompletion = Math.Round(EAC, 0);
        //        existingMetric.EstimateToComplete = Math.Round(ETC, 0);
        //        existingMetric.VarianceAtCompletion = Math.Round(VAC, 0);
        //        existingMetric.EstimateDurationAtCompletion = Math.Round(EDAC, 0);
        //        existingMetric.UpdatedAt = DateTime.UtcNow;

        //        await _repo.Update(existingMetric);
        //        return _mapper.Map<NewProjectMetricResponseDTO>(existingMetric);
        //    }
        //    else
        //    {
        //        var newMetric = new ProjectMetric
        //        {
        //            ProjectId = project.Id,
        //            PlannedValue = PV,
        //            EarnedValue = EV,
        //            ActualCost = AC,
        //            BudgetAtCompletion = BAC,
        //            DurationAtCompletion = DAC,
        //            CostVariance = CV,
        //            ScheduleVariance = SV,
        //            CostPerformanceIndex = Math.Round(CPI, 3),
        //            SchedulePerformanceIndex = Math.Round(SPI, 3),
        //            EstimateAtCompletion = Math.Round(EAC, 0),
        //            EstimateToComplete = Math.Round(ETC, 0),
        //            VarianceAtCompletion = Math.Round(VAC, 0),
        //            EstimateDurationAtCompletion = Math.Round(EDAC, 0),
        //            CalculatedBy = "System",
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow,
        //        };

        //        await _repo.Add(newMetric);
        //        return _mapper.Map<NewProjectMetricResponseDTO>(newMetric);
        //    }
        //}

        //public async Task<NewProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");
        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

        //    if (tasks == null || !tasks.Any())
        //        throw new InvalidOperationException("No tasks found for the project.");

        //    decimal PV = 0; // Planned Value
        //    decimal EV = 0; // Earned Value
        //    decimal AC = 0; // Actual Cost
        //    decimal BAC = (decimal)project.Budget; // Budget At Completion
        //    decimal DAC = 0; // Duration At Completion (months)
        //    string projectStatus = "Not Started"; // New field to track status

        //    // Check if project has started based on StartDate
        //    var now = DateTime.UtcNow;
        //    bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;

        //    if (project.StartDate.HasValue && project.EndDate.HasValue)
        //    {
        //        var totalDays = (project.EndDate.Value - project.StartDate.Value).TotalDays;
        //        DAC = Math.Round((decimal)(totalDays / 30.0), 0);
        //    }

        //    // Track if any progress exists
        //    bool hasProgress = false;

        //    foreach (var task in tasks)
        //    {
        //        // Tính PV: dựa vào planned cost và ngày hiện tại trong planned range
        //        if (hasProjectStarted && task.PlannedStartDate.HasValue && task.PlannedEndDate.HasValue)
        //        {
        //            var taskStart = task.PlannedStartDate.Value.Date;
        //            var taskEnd = task.PlannedEndDate.Value.Date;
        //            if (now >= taskStart && now <= taskEnd)
        //            {
        //                var totalPlannedDuration = (taskEnd - taskStart).TotalDays;
        //                totalPlannedDuration = totalPlannedDuration == 0 ? 1 : totalPlannedDuration; // Avoid division by zero
        //                var elapsed = (now - taskStart).TotalDays;
        //                var progress = (decimal)(elapsed / totalPlannedDuration);
        //                PV += (task.PlannedCost ?? 0) * progress;
        //            }
        //            else if (now > taskEnd)
        //            {
        //                PV += task.PlannedCost ?? 0;
        //            }
        //        }

        //        // EV: dựa vào phần trăm hoàn thành * planned cost
        //        if (task.PercentComplete.HasValue && task.PlannedCost.HasValue)
        //        {
        //            EV += task.PlannedCost.Value * (task.PercentComplete.Value / 100);
        //            if (task.PercentComplete.Value > 0)
        //                hasProgress = true;
        //        }

        //        // AC: Actual Cost
        //        AC += task.ActualCost ?? 0;
        //    }

        //    // Determine project status
        //    if (!hasProjectStarted)
        //    {
        //        projectStatus = "Not Started";
        //    }
        //    else if (hasProjectStarted && !hasProgress)
        //    {
        //        projectStatus = "Started but No Progress";
        //    }
        //    else
        //    {
        //        projectStatus = "In Progress";
        //    }

        //    // Công thức hiệu suất chi phí và tiến độ
        //    decimal CV = EV - AC;
        //    decimal SV = EV - PV;
        //    decimal CPI = hasProjectStarted && AC > 0 ? EV / AC : 0;
        //    decimal SPI = hasProjectStarted && PV > 0 ? EV / PV : 0;
        //    decimal EAC = hasProjectStarted && CPI > 0 ? BAC / CPI : BAC; // Fallback to BAC if CPI is 0
        //    decimal ETC = EAC - AC;
        //    decimal VAC = BAC - EAC;
        //    decimal EDAC = hasProjectStarted && SPI > 0 ? DAC / SPI : DAC; // Fallback to DAC if SPI is 0

        //    var existingMetric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

        //    if (existingMetric != null)
        //    {
        //        existingMetric.PlannedValue = PV;
        //        existingMetric.EarnedValue = EV;
        //        existingMetric.ActualCost = AC;
        //        existingMetric.BudgetAtCompletion = BAC;
        //        existingMetric.DurationAtCompletion = DAC;
        //        existingMetric.CostVariance = CV;
        //        existingMetric.ScheduleVariance = SV;
        //        existingMetric.CostPerformanceIndex = Math.Round(CPI, 3);
        //        existingMetric.SchedulePerformanceIndex = Math.Round(SPI, 3);
        //        existingMetric.EstimateAtCompletion = Math.Round(EAC, 0);
        //        existingMetric.EstimateToComplete = Math.Round(ETC, 0);
        //        existingMetric.VarianceAtCompletion = Math.Round(VAC, 0);
        //        existingMetric.EstimateDurationAtCompletion = Math.Round(EDAC, 0);
        //       // existingMetric.ProjectStatus = projectStatus; // Add new field
        //        existingMetric.UpdatedAt = DateTime.UtcNow;

        //        await _repo.Update(existingMetric);
        //        var result = _mapper.Map<NewProjectMetricResponseDTO>(existingMetric);
        //        //result.ProjectStatus = projectStatus; // Ensure DTO includes status
        //        return result;
        //    }
        //    else
        //    {
        //        var newMetric = new ProjectMetric
        //        {
        //            ProjectId = project.Id,
        //            PlannedValue = PV,
        //            EarnedValue = EV,
        //            ActualCost = AC,
        //            BudgetAtCompletion = BAC,
        //            DurationAtCompletion = DAC,
        //            CostVariance = CV,
        //            ScheduleVariance = SV,
        //            CostPerformanceIndex = Math.Round(CPI, 3),
        //            SchedulePerformanceIndex = Math.Round(SPI, 3),
        //            EstimateAtCompletion = Math.Round(EAC, 0),
        //            EstimateToComplete = Math.Round(ETC, 0),
        //            VarianceAtCompletion = Math.Round(VAC, 0),
        //            EstimateDurationAtCompletion = Math.Round(EDAC, 0),
        //            //ProjectStatus = projectStatus, // Add new field
        //            CalculatedBy = "System",
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow,
        //        };

        //        await _repo.Add(newMetric);
        //        var result = _mapper.Map<NewProjectMetricResponseDTO>(newMetric);
        //        //result.ProjectStatus = projectStatus; // Ensure DTO includes status
        //        return result;
        //    }
        //}

        public async Task<NewProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(string projectKey) //
        {
            // Fetch project and tasks
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            if (tasks == null || !tasks.Any())
                throw new InvalidOperationException("No tasks found for the project.");

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

                var existingMetric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

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
                        CalculatedBy = "System",
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

            // Gọi GeminiService để tính toán
            var result = await _geminiService.CalculateProjectMetricsAsync(project, tasks);

            result.ProjectId = projectId;
            result.CalculatedBy = "AI";

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

        //        //var totalItems = sprintTasks.Count + sprintMilestones.Count;
        //        var totalItems = sprintTasks.Count;
        //        if (totalItems == 0)
        //        {
        //            result.Add(new
        //            {
        //                sprintId = sprint.Id,
        //                sprintName = sprint.Name,
        //                percentComplete = 0.0
        //            });
        //            continue;
        //        }

        //        double taskProgress = (double)sprintTasks.Sum(t => t.PercentComplete ?? 0);
        //        double percentComplete = taskProgress / totalItems;

        //        result.Add(new
        //        {
        //            sprintId = sprint.Id,
        //            sprintName = sprint.Name,
        //            percentComplete = Math.Round(percentComplete, 2)
        //        });
        //    }

        //    return result;
        //}

        public async Task<List<object>> GetProgressDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            var result = new List<object>();

            foreach (var sprint in sprints)
            {
                var sprintTasks = tasks.Where(t => t.SprintId == sprint.Id).ToList();

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

                // Weighted calculation
                decimal totalPlanned = sprintTasks.Sum(t => t.PlannedHours ?? 0);
                if (totalPlanned == 0)
                {
                    // Nếu không có planned hours thì fallback về average % complete
                    decimal avgPercent = sprintTasks.Average(t => t.PercentComplete ?? 0);
                    result.Add(new
                    {
                        sprintId = sprint.Id,
                        sprintName = sprint.Name,
                        percentComplete = Math.Round(avgPercent, 2)
                    });
                }
                else
                {
                    decimal weightedProgress = sprintTasks.Sum(t =>
                        (t.PlannedHours ?? 0) * (t.PercentComplete ?? 0));
                    decimal percentComplete = weightedProgress / totalPlanned;

                    result.Add(new
                    {
                        sprintId = sprint.Id,
                        sprintName = sprint.Name,
                        percentComplete = Math.Round(percentComplete, 2)
                    });
                }
            }

            return result;
        }


        //public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    //var latestMetric = await _repo.GetLatestByProjectIdAsync(project.Id);
        //    var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

        //    // Tính tiến độ theo phần trăm task đã hoàn thành
        //    int tasksToBeCompleted = tasks.Count(t => t.Status != "DONE");
        //    int overdueTasks = tasks.Count(t =>
        //        t.PlannedEndDate.HasValue &&
        //        t.PlannedEndDate.Value < DateTime.UtcNow &&
        //        t.Status != "DONE");

        //    double progress = tasks.Any()
        //        ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
        //        : 0;

        //    string timeStatus = "On track";
        //    decimal costStatus = 0;
        //    var costDto = new NewProjectMetricResponseDTO();

        //    if (metric != null)
        //    {
        //        // Làm tròn chỉ số và map DTO mới
        //        costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);

        //        costDto.CostPerformanceIndex = Math.Round(costDto.CostPerformanceIndex, 3);
        //        costDto.SchedulePerformanceIndex = Math.Round(costDto.SchedulePerformanceIndex, 3);
        //        costDto.EstimateDurationAtCompletion = Math.Round(costDto.EstimateDurationAtCompletion, 1);
        //        costDto.EstimateAtCompletion = Math.Round(costDto.EstimateAtCompletion, 0);
        //        costDto.EstimateToComplete = Math.Round(costDto.EstimateToComplete, 0);
        //        costDto.VarianceAtCompletion = Math.Round(costDto.VarianceAtCompletion, 0);

        //        // Tính trạng thái thời gian: nếu SPI < 1 thì đang chậm
        //        if (progress == 0 && costDto.SchedulePerformanceIndex == 0)
        //        {
        //            timeStatus = "Project not started";
        //        }
        //        else if (costDto.SchedulePerformanceIndex < 1)
        //        {
        //            timeStatus = $"{Math.Round((1 - (double)costDto.SchedulePerformanceIndex) * 100, 2)}% behind";
        //        }
        //        else if (costDto.SchedulePerformanceIndex > 1)
        //        {
        //            timeStatus = $"{Math.Round(((double)costDto.SchedulePerformanceIndex - 1) * 100, 2)}% ahead";
        //        }
        //        else 
        //        {
        //            timeStatus = "On time";
        //        }

        //        costStatus = costDto.CostPerformanceIndex;
        //    }

        //    return new ProjectHealthDTO
        //    {
        //        TimeStatus = timeStatus,
        //        TasksToBeCompleted = tasksToBeCompleted,
        //        OverdueTasks = overdueTasks,
        //        ProgressPercent = Math.Round(progress, 2),
        //        CostStatus = costStatus,
        //        Cost = costDto
        //    };
        //}

        //public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

        //    // Tính tiến độ theo phần trăm task đã hoàn thành
        //    int tasksToBeCompleted = tasks.Count(t => t.Status != "DONE");
        //    int overdueTasks = tasks.Count(t =>
        //        t.PlannedEndDate.HasValue &&
        //        t.PlannedEndDate.Value < DateTime.UtcNow &&
        //        t.Status != "DONE");

        //    double progress = tasks.Any()
        //        ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
        //        : 0;

        //    string timeStatus = "Not Started";
        //    decimal costStatus = 0;
        //    var costDto = new NewProjectMetricResponseDTO();

        //    var now = DateTime.UtcNow;
        //    bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;

        //    if (metric != null)
        //    {
        //        // Map DTO and round metrics
        //        costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);
        //        costDto.CostPerformanceIndex = Math.Round(costDto.CostPerformanceIndex, 3);
        //        costDto.SchedulePerformanceIndex = Math.Round(costDto.SchedulePerformanceIndex, 3);
        //        costDto.EstimateDurationAtCompletion = Math.Round(costDto.EstimateDurationAtCompletion, 1);
        //        costDto.EstimateAtCompletion = Math.Round(costDto.EstimateAtCompletion, 0);
        //        costDto.EstimateToComplete = Math.Round(costDto.EstimateToComplete, 0);
        //        costDto.VarianceAtCompletion = Math.Round(costDto.VarianceAtCompletion, 0);

        //        // Determine time status
        //        if (!hasProjectStarted)
        //        {
        //            timeStatus = "Not Started";
        //        }
        //        else if (hasProjectStarted && progress == 0)
        //        {
        //            timeStatus = "Started but No Progress";
        //        }
        //        else
        //        {
        //            if (costDto.SchedulePerformanceIndex < 1)
        //            {
        //                timeStatus = $"{Math.Round((1 - (double)costDto.SchedulePerformanceIndex) * 100, 2)}% behind";
        //            }
        //            else if (costDto.SchedulePerformanceIndex > 1)
        //            {
        //                timeStatus = $"{Math.Round(((double)costDto.SchedulePerformanceIndex - 1) * 100, 2)}% ahead";
        //            }
        //            else
        //            {
        //                timeStatus = "On Time";
        //            }
        //        }

        //        costStatus = costDto.CostPerformanceIndex;
        //    }

        //    return new ProjectHealthDTO
        //    {
        //        TimeStatus = timeStatus,
        //        TasksToBeCompleted = tasksToBeCompleted,
        //        OverdueTasks = overdueTasks,
        //        ProgressPercent = Math.Round(progress, 2),
        //        CostStatus = costStatus,
        //        Cost = costDto
        //    };
        //}

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
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id)
                ?? throw new Exception("Project has no tasks");

            // Fetch configurations
            var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");
            var aheadThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("ahead_threshold");
            var behindThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("behind_threshold");
            double aheadThreshold = aheadThresholdConfig != null ? double.Parse(aheadThresholdConfig.ValueConfig) : 5.0;
            double behindThreshold = behindThresholdConfig != null ? double.Parse(behindThresholdConfig.ValueConfig) : -5.0;

            var today = DateTime.UtcNow.Date;

            var taskInfos = tasks.Select(task => new
            {
                Task = task,
                PlannedTotalCost = (task.PlannedCost ?? 0) + (task.PlannedResourceCost ?? 0),
                AvgPercentComplete = (double)(task.PercentComplete ?? 0)
            }).ToList();

            decimal totalPlannedCost = taskInfos.Sum(t => t.PlannedTotalCost);
            decimal plannedCostTillToday = taskInfos
                .Where(t => t.Task.PlannedEndDate.HasValue && t.Task.PlannedEndDate.Value <= today)
                .Sum(t => t.PlannedTotalCost);

            double plannedCompletion;
            if (totalPlannedCost > 0)
            {
                plannedCompletion = (double)(plannedCostTillToday / totalPlannedCost) * 100;
            }
            else
            {
                int totalTasks = taskInfos.Count;
                int expectedCompletedTasks = taskInfos
                    .Count(t => t.Task.PlannedEndDate.HasValue && t.Task.PlannedEndDate.Value <= today);

                plannedCompletion = totalTasks == 0 ? 0 : (double)expectedCompletedTasks / totalTasks * 100;
            }

            double actualCompletion = taskInfos.Count == 0 ? 0 : taskInfos.Average(t => t.AvgPercentComplete);

            string status;
            double diff = actualCompletion - plannedCompletion;
            if (diff > aheadThreshold)
            {
                status = healthStatusCategories.FirstOrDefault(c => c.Name == "AHEAD")?.Label ?? "Ahead";
            }
            else if (diff < behindThreshold)
            {
                status = healthStatusCategories.FirstOrDefault(c => c.Name == "BEHIND")?.Label ?? "Behind";
            }
            else
            {
                status = healthStatusCategories.FirstOrDefault(c => c.Name == "ON_TIME")?.Label ?? "On Time";
            }

            return new
            {
                plannedCompletion = Math.Round(plannedCompletion, 2),
                actualCompletion = Math.Round(actualCompletion, 2),
                status
            };
        }

        //public async Task<object> GetTimeDashboardAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");
        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id)
        //        ?? throw new Exception("Project has no tasks");

        //    // Fetch configurations
        //    var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");
        //    var aheadThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("ahead_threshold");
        //    var behindThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("behind_threshold");
        //    double aheadThreshold = aheadThresholdConfig != null ? double.Parse(aheadThresholdConfig.ValueConfig) : 5.0;
        //    double behindThreshold = behindThresholdConfig != null ? double.Parse(behindThresholdConfig.ValueConfig) : -5.0;

        //    var today = DateTime.UtcNow.Date;

        //    var taskInfos = tasks.Select(task => new
        //    {
        //        Task = task,
        //        PlannedCost = task.PlannedCost ?? 0,
        //        AvgPercentComplete = (double)(task.PercentComplete ?? 0)
        //    }).ToList();

        //    decimal totalPlannedCost = taskInfos.Sum(t => t.PlannedCost);
        //    decimal plannedCostTillToday = taskInfos
        //        .Where(t => t.Task.PlannedEndDate.HasValue && t.Task.PlannedEndDate.Value <= today)
        //        .Sum(t => t.PlannedCost);

        //    double plannedCompletion;
        //    if (totalPlannedCost > 0)
        //    {
        //        plannedCompletion = (double)(plannedCostTillToday / totalPlannedCost) * 100;
        //    }
        //    else
        //    {
        //        int totalTasks = taskInfos.Count;
        //        int expectedCompletedTasks = taskInfos
        //            .Count(t => t.Task.PlannedEndDate.HasValue && t.Task.PlannedEndDate.Value <= today);

        //        plannedCompletion = totalTasks == 0 ? 0 : (double)expectedCompletedTasks / totalTasks * 100;
        //    }

        //    double actualCompletion = taskInfos.Count == 0 ? 0 : taskInfos.Average(t => t.AvgPercentComplete);

        //    string status;
        //    double diff = actualCompletion - plannedCompletion;
        //    if (diff > aheadThreshold)
        //    {
        //        status = healthStatusCategories.FirstOrDefault(c => c.Name == "AHEAD")?.Label ?? "Ahead";
        //    }
        //    else if (diff < behindThreshold)
        //    {
        //        status = healthStatusCategories.FirstOrDefault(c => c.Name == "BEHIND")?.Label ?? "Behind";
        //    }
        //    else
        //    {
        //        status = healthStatusCategories.FirstOrDefault(c => c.Name == "ON_TIME")?.Label ?? "On Time";
        //    }

        //    return new
        //    {
        //        plannedCompletion = Math.Round(plannedCompletion, 2),
        //        actualCompletion = Math.Round(actualCompletion, 2),
        //        status
        //    };
        //}

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
            var entity = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

            return _mapper.Map<NewProjectMetricResponseDTO>(entity);
        }

        public async Task<NewProjectMetricResponseDTO> GetProjectForecastByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var entity = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "AI");

            return _mapper.Map<NewProjectMetricResponseDTO>(entity);
        }


        //public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

        //    // Fetch configurations
        //    var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");
        //    var spiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("spi_warning_threshold");
        //    var cpiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("cpi_warning_threshold");
        //    decimal spiWarningThreshold = spiWarningThresholdConfig != null ? decimal.Parse(spiWarningThresholdConfig.ValueConfig) : 0.9m;
        //    decimal cpiWarningThreshold = cpiWarningThresholdConfig != null ? decimal.Parse(cpiWarningThresholdConfig.ValueConfig) : 0.9m;

        //    // Calculate tasks metrics
        //    int tasksToBeCompleted = tasks.Count(t => t.Status != "DONE");
        //    int overdueTasks = tasks.Count(t =>
        //        t.PlannedEndDate.HasValue &&
        //        t.PlannedEndDate.Value < DateTime.UtcNow &&
        //        t.Status != "DONE");

        //    double progress = tasks.Any()
        //        ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
        //        : 0;

        //    // Use project.Status directly
        //    string projectStatus = project.Status ?? "Not Started";
        //    string timeStatus = healthStatusCategories.FirstOrDefault(c => c.Name == "NO_PROGRESS")?.Label ?? "No Progress";
        //    decimal costStatus = 0;
        //    var costDto = new NewProjectMetricResponseDTO();

        //    var now = DateTime.UtcNow;
        //    bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;
        //    bool hasProgress = tasks.Any(t => t.PercentComplete > 0);

        //    if (hasProjectStarted && progress > 0 && metric != null)
        //    {
        //        if (metric.SchedulePerformanceIndex < spiWarningThreshold)
        //        {
        //            timeStatus = $"{Math.Round((1 - (double)metric.SchedulePerformanceIndex) * 100, 2)}% behind";
        //        }
        //        else if (metric.SchedulePerformanceIndex > 1)
        //        {
        //            timeStatus = $"{Math.Round(((double)metric.SchedulePerformanceIndex - 1) * 100, 2)}% ahead";
        //        }
        //        else
        //        {
        //            timeStatus = healthStatusCategories.FirstOrDefault(c => c.Name == "ON_TIME")?.Label ?? "On Time";
        //        }
        //    }

        //    bool showAlert = false;
        //    if (metric != null)
        //    {
        //        // Map DTO and round metrics
        //        costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);
        //        costDto.CostPerformanceIndex = Math.Round(costDto.CostPerformanceIndex, 3);
        //        costDto.SchedulePerformanceIndex = Math.Round(costDto.SchedulePerformanceIndex, 3);
        //        costDto.EstimateDurationAtCompletion = Math.Round(costDto.EstimateDurationAtCompletion, 1);
        //        costDto.EstimateAtCompletion = Math.Round(costDto.EstimateAtCompletion, 0);
        //        costDto.EstimateToComplete = Math.Round(costDto.EstimateToComplete, 0);
        //        costDto.VarianceAtCompletion = Math.Round(costDto.VarianceAtCompletion, 0);
        //        costDto.ProjectStatus = projectStatus;

        //        costStatus = costDto.CostPerformanceIndex;
        //        showAlert = hasProjectStarted && (
        //            costDto.SchedulePerformanceIndex < spiWarningThreshold ||
        //            costDto.CostPerformanceIndex < cpiWarningThreshold
        //        );
        //    }

        //    return new ProjectHealthDTO
        //    {
        //        ProjectStatus = projectStatus,
        //        TimeStatus = timeStatus,
        //        TasksToBeCompleted = tasksToBeCompleted,
        //        OverdueTasks = overdueTasks,
        //        ProgressPercent = Math.Round(progress, 2),
        //        CostStatus = costStatus,
        //        Cost = costDto,
        //        ShowAlert = showAlert
        //    };
        //}

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

            return new CostDashboardResponseDTO
            {
                ActualCost = actualCost,
                ActualTaskCost = actualTaskCost,
                ActualResourceCost = actualResourceCost,
                PlannedCost = plannedCost,
                PlannedTaskCost = plannedTaskCost, 
                PlannedResourceCost = plannedResourceCost,
                Budget = project.Budget ?? 0
            };
        }


        //public async Task<CostDashboardResponseDTO> GetCostDashboardAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

        //    // Calculate Task Costs from tasks table
        //    decimal actualTaskCost = tasks.Sum(t => t.ActualCost ?? 0);
        //    decimal plannedTaskCost = tasks.Sum(t => t.PlannedCost ?? 0);

        //    return new CostDashboardResponseDTO
        //    {
        //        ActualTaskCost = actualTaskCost,
        //        PlannedTaskCost = plannedTaskCost,
        //        ActualResourceCost = actualTaskCost, 
        //        PlannedResourceCost = plannedTaskCost, 
        //        ActualCost = actualTaskCost,
        //        PlannedCost = plannedTaskCost,
        //        Budget = project.Budget ?? 0
        //    };
        //}

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

            // Fetch configurations
            var healthStatusCategories = await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("", "health_status");
            var spiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("spi_warning_threshold");
            var cpiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("cpi_warning_threshold");
            var minDaysForAlertConfig = await _systemConfigRepo.GetByConfigKeyAsync("minimum_days_for_alert");
            var minProgressForAlertConfig = await _systemConfigRepo.GetByConfigKeyAsync("minimum_progress_for_alert");

            // Parse configuration values with defaults
            decimal spiWarningThreshold = spiWarningThresholdConfig != null ? decimal.Parse(spiWarningThresholdConfig.ValueConfig) : 0.9m;
            decimal cpiWarningThreshold = cpiWarningThresholdConfig != null ? decimal.Parse(cpiWarningThresholdConfig.ValueConfig) : 0.9m;
            int minDaysForAlert = minDaysForAlertConfig != null ? int.Parse(minDaysForAlertConfig.ValueConfig) : 7;
            double minProgressForAlert = minProgressForAlertConfig != null ? double.Parse(minProgressForAlertConfig.ValueConfig) : 5.0;

            // Calculate tasks metrics
            var doneStatus = (await _dynamicCategoryRepo.GetByNameOrCategoryGroupAsync("DONE", "task_status"))?.FirstOrDefault()?.Name ?? "DONE";
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
            string timeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, "BEHIND", StringComparison.OrdinalIgnoreCase))?.Label ?? "Behind";
            string costStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, "OVER_BUDGET", StringComparison.OrdinalIgnoreCase))?.Label ?? "Over Budget";
            var costDto = new NewProjectMetricResponseDTO();

            var now = DateTime.UtcNow;
            bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;
            bool hasProgress = tasks.Any(t => (t.PercentComplete ?? 0) > 0);
            bool isBeyondMinDays = project.StartDate.HasValue && (now - project.StartDate.Value).TotalDays >= minDaysForAlert;

            if (metric != null && hasProjectStarted)
            {
                // Calculate timeStatus based on SPI
                //if (metric.SchedulePerformanceIndex == 0)
                //{
                //    timeStatus = "100% behind";
                //}
                if (metric.SchedulePerformanceIndex < spiWarningThreshold)
                {
                    timeStatus = $"{Math.Round((1 - (double)metric.SchedulePerformanceIndex) * 100, 2)}% behind";
                }
                else if (metric.SchedulePerformanceIndex > spiWarningThreshold)
                {
                    timeStatus = $"{Math.Round(((double)metric.SchedulePerformanceIndex - 1) * 100, 2)}% ahead";
                }
                else
                {
                    timeStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, "ON_TIME", StringComparison.OrdinalIgnoreCase))?.Label ?? "On Time";
                }

                // Calculate costStatus based on CPI

                if (metric.CostPerformanceIndex < cpiWarningThreshold)
                {
                    costStatus = $"{Math.Round((1 - (double)metric.CostPerformanceIndex) * 100, 2)}% over budget";
                }
                else if (metric.CostPerformanceIndex > cpiWarningThreshold)
                {
                    costStatus = $"{Math.Round(((double)metric.CostPerformanceIndex - 1) * 100, 2)}% under budget";
                }
                else
                {
                    costStatus = healthStatusCategories.FirstOrDefault(c => string.Equals(c.Name, "ON_BUDGET", StringComparison.OrdinalIgnoreCase))?.Label ?? "On Budget";
                }
            }

            bool showAlert = false;
            if (metric != null && hasProjectStarted && isBeyondMinDays && progress >= minProgressForAlert)
            {
                // Map DTO and round metrics
                costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);
                costDto.CostPerformanceIndex = Math.Round(costDto.CostPerformanceIndex, 3);
                costDto.SchedulePerformanceIndex = Math.Round(costDto.SchedulePerformanceIndex, 3);
                costDto.EstimateDurationAtCompletion = Math.Round(costDto.EstimateDurationAtCompletion, 1);
                costDto.EstimateAtCompletion = Math.Round(costDto.EstimateAtCompletion, 0);
                costDto.EstimateToComplete = Math.Round(costDto.EstimateToComplete, 0);
                costDto.VarianceAtCompletion = Math.Round(costDto.VarianceAtCompletion, 0);
                //costDto.ProjectStatus = projectStatus;

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
