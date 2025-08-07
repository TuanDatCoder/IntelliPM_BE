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

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, ITaskAssignmentRepository taskAssignmentRepo, IProjectMemberRepository projectMemberRepo, IProjectRecommendationRepository projectRecommendationRepo, IDynamicCategoryRepository dynamicCategoryRepo, IChatGPTService chatGPTService, ISubtaskRepository subtaskRepo)
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

        public async Task<NewProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            if (tasks == null || !tasks.Any())
                throw new InvalidOperationException("No tasks found for the project.");

            decimal PV = 0; // Planned Value
            decimal EV = 0; // Earned Value
            decimal AC = 0; // Actual Cost
            decimal BAC = (decimal)project.Budget; // Budget At Completion
            decimal DAC = 0; // Duration At Completion (months)
            string projectStatus = "Not Started"; // New field to track status

            // Check if project has started based on StartDate
            var now = DateTime.UtcNow;
            bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;

            if (project.StartDate.HasValue && project.EndDate.HasValue)
            {
                var totalDays = (project.EndDate.Value - project.StartDate.Value).TotalDays;
                DAC = Math.Round((decimal)(totalDays / 30.0), 0);
            }

            // Track if any progress exists
            bool hasProgress = false;

            foreach (var task in tasks)
            {
                // Tính PV: dựa vào planned cost và ngày hiện tại trong planned range
                if (hasProjectStarted && task.PlannedStartDate.HasValue && task.PlannedEndDate.HasValue)
                {
                    var taskStart = task.PlannedStartDate.Value.Date;
                    var taskEnd = task.PlannedEndDate.Value.Date;
                    if (now >= taskStart && now <= taskEnd)
                    {
                        var totalPlannedDuration = (taskEnd - taskStart).TotalDays;
                        totalPlannedDuration = totalPlannedDuration == 0 ? 1 : totalPlannedDuration; // Avoid division by zero
                        var elapsed = (now - taskStart).TotalDays;
                        var progress = (decimal)(elapsed / totalPlannedDuration);
                        PV += (task.PlannedCost ?? 0) * progress;
                    }
                    else if (now > taskEnd)
                    {
                        PV += task.PlannedCost ?? 0;
                    }
                }

                // EV: dựa vào phần trăm hoàn thành * planned cost
                if (task.PercentComplete.HasValue && task.PlannedCost.HasValue)
                {
                    EV += task.PlannedCost.Value * (task.PercentComplete.Value / 100);
                    if (task.PercentComplete.Value > 0)
                        hasProgress = true;
                }

                // AC: Actual Cost
                AC += task.ActualCost ?? 0;
            }

            // Determine project status
            if (!hasProjectStarted)
            {
                projectStatus = "Not Started";
            }
            else if (hasProjectStarted && !hasProgress)
            {
                projectStatus = "Started but No Progress";
            }
            else
            {
                projectStatus = "In Progress";
            }

            // Công thức hiệu suất chi phí và tiến độ
            decimal CV = EV - AC;
            decimal SV = EV - PV;
            decimal CPI = hasProjectStarted && AC > 0 ? EV / AC : 0;
            decimal SPI = hasProjectStarted && PV > 0 ? EV / PV : 0;
            decimal EAC = hasProjectStarted && CPI > 0 ? BAC / CPI : BAC; // Fallback to BAC if CPI is 0
            decimal ETC = EAC - AC;
            decimal VAC = BAC - EAC;
            decimal EDAC = hasProjectStarted && SPI > 0 ? DAC / SPI : DAC; // Fallback to DAC if SPI is 0

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
               // existingMetric.ProjectStatus = projectStatus; // Add new field
                existingMetric.UpdatedAt = DateTime.UtcNow;

                await _repo.Update(existingMetric);
                var result = _mapper.Map<NewProjectMetricResponseDTO>(existingMetric);
                //result.ProjectStatus = projectStatus; // Ensure DTO includes status
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
                    //ProjectStatus = projectStatus, // Add new field
                    CalculatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await _repo.Add(newMetric);
                var result = _mapper.Map<NewProjectMetricResponseDTO>(newMetric);
                //result.ProjectStatus = projectStatus; // Ensure DTO includes status
                return result;
            }
        }

        public async Task<NewProjectMetricResponseDTO> CalculateProjectMetricsViewAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            if (tasks == null || !tasks.Any())
                throw new InvalidOperationException("No tasks found for the project.");

            decimal PV = 0; // Planned Value
            decimal EV = 0; // Earned Value
            decimal AC = 0; // Actual Cost
            decimal BAC = (decimal)project.Budget; // Budget At Completion
            decimal DAC = 12; // Duration At Completion (tháng)

            foreach (var task in tasks)
            {
                // Tính PV: dựa vào planned cost và ngày hiện tại trong planned range
                if (task.PlannedStartDate.HasValue && task.PlannedEndDate.HasValue)
                {
                    var now = DateTime.UtcNow;
                    if (now >= task.PlannedStartDate && now <= task.PlannedEndDate)
                    {
                        var totalPlannedDuration = (task.PlannedEndDate - task.PlannedStartDate)?.TotalDays ?? 1;
                        var elapsed = (now - task.PlannedStartDate)?.TotalDays ?? 0;
                        var progress = (decimal)(elapsed / totalPlannedDuration);
                        PV += (task.PlannedCost ?? 0) * progress;
                    }
                    else if (now > task.PlannedEndDate)
                    {
                        PV += task.PlannedCost ?? 0;
                    }
                }

                // EV: dựa vào phần trăm hoàn thành * planned cost
                if (task.PercentComplete.HasValue && task.PlannedCost.HasValue)
                {
                    EV += task.PlannedCost.Value * (task.PercentComplete.Value / 100);
                }

                // AC: Actual Cost
                AC += task.ActualCost ?? 0;
            }

            decimal CV = EV - AC;
            decimal SV = EV - PV;
            decimal CPI = AC == 0 ? 0 : EV / AC;
            decimal SPI = PV == 0 ? 0 : EV / PV;
            decimal EAC = CPI == 0 ? 0 : BAC / CPI;
            decimal ETC = EAC - AC;
            decimal VAC = BAC - EAC;
            decimal EDAC = SPI == 0 ? DAC : DAC / SPI;

            return new NewProjectMetricResponseDTO
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
                EstimateDurationAtCompletion = Math.Round(EDAC, 1),
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<ProjectMetricRequestDTO> CalculateProjectMetricsByAIAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            var result = await _geminiService.CalculateProjectMetricsAsync(project, tasks);
            if (result == null)
                throw new Exception("AI did not return valid project metrics");

            //var result = await _chatGPTService.CalculateProjectMetricsAsync(project, tasks);
            //if (result == null)
            //    throw new Exception("AI did not return valid project metrics");

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
            //if (result.Suggestions != null && result.Suggestions.Any())
            //{
            //    var allTasks = tasks.ToDictionary(t => t.Title?.Trim(), t => t.Id);

            //    foreach (var suggestion in result.Suggestions)
            //    {
            //        foreach (var related in suggestion.RelatedTasks)
            //        {
            //            var taskTitle = related.TaskTitle?.Trim();
            //            if (string.IsNullOrEmpty(taskTitle) || !allTasks.ContainsKey(taskTitle))
            //                continue;

            //            var taskId = allTasks[taskTitle].ToString();
            //            var type = suggestion.Label ?? suggestion.Reason ?? "AI";
            //            var recommendationText = related.SuggestedAction ?? suggestion.Message;

            //            var existingRec = await _projectRecommendationRepo.GetByProjectIdTaskIdTypeAsync(project.Id, taskId, type);

            //            if (existingRec != null)
            //            {
            //                existingRec.Recommendation = recommendationText;
            //                existingRec.CreatedAt = DateTime.UtcNow;

            //                await _projectRecommendationRepo.Update(existingRec);
            //            }
            //            else
            //            {
            //                var rec = new ProjectRecommendation
            //                {
            //                    ProjectId = project.Id,
            //                    TaskId = taskId,
            //                    Type = type,
            //                    Recommendation = recommendationText,
            //                    CreatedAt = DateTime.UtcNow,
            //                };

            //                await _projectRecommendationRepo.Add(rec);
            //            }
            //        }
            //    }
            //}

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

        public async Task<CostDashboardResponseDTO> GetCostDashboardAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);

            // Lấy tất cả subtasks theo danh sách task Id
            var taskIds = tasks.Select(t => t.Id).ToList();
            var allSubtasks = new List<Subtask>();

            foreach (var task in tasks)
            {
                var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(task.Id);
                allSubtasks.AddRange(subtasks);
            }

            // Lấy danh sách ProjectMember trước để dùng lại nhiều lần
            var projectMembers = await _projectMemberRepo.GetByProjectIdAsync(project.Id);

            // Tính Task Cost
            decimal actualTaskCost = tasks.Sum(t => t.ActualCost ?? 0);
            decimal plannedTaskCost = tasks.Sum(t => t.PlannedCost ?? 0);

            // Tính Resource Cost
            decimal actualResourceCost = 0;
            decimal plannedResourceCost = 0;

            foreach (var subtask in allSubtasks)
            {
                var member = projectMembers.FirstOrDefault(m =>
                    m.AccountId == subtask.AssignedBy && m.ProjectId == project.Id);

                var hourlyRate = member?.HourlyRate ?? 0;

                actualResourceCost += (decimal)(subtask.ActualHours ?? 0) * hourlyRate;
                plannedResourceCost += (decimal)(subtask.PlannedHours ?? 0) * hourlyRate;
            }

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

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var metric = await _repo.GetByProjectIdAndCalculatedByAsync(project.Id, "System");

            // Tính tiến độ theo phần trăm task đã hoàn thành
            int tasksToBeCompleted = tasks.Count(t => t.Status != "DONE");
            int overdueTasks = tasks.Count(t =>
                t.PlannedEndDate.HasValue &&
                t.PlannedEndDate.Value < DateTime.UtcNow &&
                t.Status != "DONE");

            double progress = tasks.Any()
                ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
                : 0;

            string timeStatus = "Not Started";
            decimal costStatus = 0;
            var costDto = new NewProjectMetricResponseDTO();

            var now = DateTime.UtcNow;
            bool hasProjectStarted = project.StartDate.HasValue && project.StartDate.Value.Date <= now.Date;

            if (metric != null)
            {
                // Map DTO and round metrics
                costDto = _mapper.Map<NewProjectMetricResponseDTO>(metric);
                costDto.CostPerformanceIndex = Math.Round(costDto.CostPerformanceIndex, 3);
                costDto.SchedulePerformanceIndex = Math.Round(costDto.SchedulePerformanceIndex, 3);
                costDto.EstimateDurationAtCompletion = Math.Round(costDto.EstimateDurationAtCompletion, 1);
                costDto.EstimateAtCompletion = Math.Round(costDto.EstimateAtCompletion, 0);
                costDto.EstimateToComplete = Math.Round(costDto.EstimateToComplete, 0);
                costDto.VarianceAtCompletion = Math.Round(costDto.VarianceAtCompletion, 0);

                // Determine time status
                if (!hasProjectStarted)
                {
                    timeStatus = "Not Started";
                }
                else if (hasProjectStarted && progress == 0)
                {
                    timeStatus = "Started but No Progress";
                }
                else
                {
                    if (costDto.SchedulePerformanceIndex < 1)
                    {
                        timeStatus = $"{Math.Round((1 - (double)costDto.SchedulePerformanceIndex) * 100, 2)}% behind";
                    }
                    else if (costDto.SchedulePerformanceIndex > 1)
                    {
                        timeStatus = $"{Math.Round(((double)costDto.SchedulePerformanceIndex - 1) * 100, 2)}% ahead";
                    }
                    else
                    {
                        timeStatus = "On Time";
                    }
                }

                costStatus = costDto.CostPerformanceIndex;
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

            var projectMembers = await _projectMemberRepo.GetByProjectIdAsync(project.Id);

            var subtasks = new List<Subtask>();
            foreach (var task in tasks)
            {
                var subtaskList = await _subtaskRepo.GetSubtaskByTaskIdAsync(task.Id);
                subtasks.AddRange(subtaskList);
            }

            // Tạo dictionary taskId → subtasks
            var subtasksByTaskId = subtasks
                .GroupBy(s => s.TaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var today = DateTime.UtcNow;

            var taskInfos = tasks.Select(task =>
            {
                var subtasks = subtasksByTaskId.ContainsKey(task.Id) ? subtasksByTaskId[task.Id] : new List<Subtask>();

                // Tính PlannedCost từ subtasks
                decimal plannedCost = subtasks.Sum(s =>
                {
                    var hourlyRate = projectMembers
                        .FirstOrDefault(m => m.AccountId == s.AssignedBy && m.ProjectId == project.Id)?.HourlyRate ?? 0;
                    return (decimal)(s.PlannedHours ?? 0) * hourlyRate;
                });

                // Tính % hoàn thành trung bình subtasks
                double avgPercentComplete = (double)(subtasks.Count > 0
                    ? subtasks.Average(s => s.PercentComplete ?? 0)
                    : (task.PercentComplete ?? 0)); // fallback nếu không có subtask

                return new
                {
                    Task = task,
                    PlannedCost = plannedCost,
                    AvgPercentComplete = avgPercentComplete
                };
            }).ToList();

            decimal totalPlannedCost = taskInfos.Sum(t => t.PlannedCost);
            decimal plannedCostTillToday = taskInfos
                .Where(t => t.Task.PlannedEndDate.HasValue && t.Task.PlannedEndDate.Value <= today)
                .Sum(t => t.PlannedCost);

            // Tính planned completion
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

            // Tính actual completion: trung bình % hoàn thành các task (từ subtask)
            double actualCompletion = taskInfos.Count == 0 ? 0 : taskInfos.Average(t => t.AvgPercentComplete);

            // Phân loại status
            //double diff = actualCompletion - plannedCompletion;
            //string status = diff > 5 ? "Ahead" : (diff < -5 ? "Behind" : "On Time");
            string status;
            if (actualCompletion == 0 && plannedCompletion >= 0)
            {
                status = "Not Started";
            }
            else
            {
                double diff = actualCompletion - plannedCompletion;
                status = diff > 5 ? "Ahead" : (diff < -5 ? "Behind" : "On Time");
            }

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
    }
}
