using AutoMapper;
using IntelliPM.Data.DTOs.ProjectRecommendation.Request;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.ProjectRecommendationRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Services.ProjectMetricServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectRecommendationServices
{
    public class ProjectRecommendationService : IProjectRecommendationService
    {
        private readonly IMapper _mapper;
        private readonly IProjectRecommendationRepository _projectRecommendationRepo;
        private readonly IProjectMetricRepository _projectMetricRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly ILogger<ProjectRecommendationService> _logger;
        private readonly IGeminiService _geminiService;
        private readonly IDynamicCategoryHelper _dynamicCategoryHelper;

        public ProjectRecommendationService(IMapper mapper, IProjectMetricRepository projectMetricRepo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectRecommendationService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, IProjectRecommendationRepository projectRecommendationRepo, ISubtaskRepository subtaskRepo, IDynamicCategoryHelper dynamicCategoryHelper)
        {
            _mapper = mapper;
            _projectMetricRepo = projectMetricRepo;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _sprintRepo = sprintRepo;
            _milestoneRepo = milestoneRepo;
            _projectRecommendationRepo = projectRecommendationRepo;
            _subtaskRepo = subtaskRepo;
            _logger = logger;
            _geminiService = geminiService;
            _dynamicCategoryHelper = dynamicCategoryHelper;
        }

        public async Task CreateAsync(ProjectRecommendationRequestDTO dto)
        {
            var entity = new ProjectRecommendation
            {
                ProjectId = dto.ProjectId,
                Type = dto.Type,
                Recommendation = dto.Recommendation,
                SuggestedChanges = dto.SuggestedChanges,
                Details = dto.Details,
                CreatedAt = DateTime.UtcNow
            };

            await _projectRecommendationRepo.Add(entity);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var rec = await _projectRecommendationRepo.GetByIdAsync(id);
            if (rec == null)
                throw new Exception("Recommendation not found");

            await _projectRecommendationRepo.Delete(rec);
        }

        public async Task<List<AIRecommendationDTO>> GenerateProjectRecommendationsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var metric = await _projectMetricRepo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);
            var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);

            if (metric == null)
                throw new Exception("ProjectMetric not found");

            // Nếu dự án không gặp vấn đề về SPI hoặc CPI thì không cần gọi AI
            if (metric.SchedulePerformanceIndex >= 1 && metric.CostPerformanceIndex >= 1)
                return new List<AIRecommendationDTO>();

            // Gọi AI sinh recommendations
            var recommendations = await _geminiService.GenerateProjectRecommendationsAsync(
                project,
                metric,
                tasks,
                sprints,
                milestones,
                subtasks
            );

            return recommendations ?? new List<AIRecommendationDTO>();
        }

        public async Task<List<ProjectRecommendationResponseDTO>> GetByProjectKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");
            var recommendations = await _projectRecommendationRepo.GetByProjectIdAsync(project.Id);
            return _mapper.Map<List<ProjectRecommendationResponseDTO>>(recommendations);
        }

        //public async Task<SimulatedMetricDTO> SimulateProjectMetricsAfterRecommendationsAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
        //        ?? throw new Exception("Project not found");

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
        //    var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
        //    var metric = await _projectMetricRepo.GetByProjectIdAsync(project.Id);
        //    var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);
        //    var approvedRecommendtions = await _projectRecommendationRepo.GetByProjectIdAsync(project.Id);

        //    if (metric == null)
        //        throw new Exception("ProjectMetric not found");

        //    // Gọi AI sinh forecast
        //    var forecast = await _geminiService.SimulateProjectMetricsAfterRecommendationsAsync(
        //        project,
        //        metric,
        //        tasks,
        //        sprints,
        //        milestones,
        //        subtasks,
        //        approvedRecommendtions
        //    );

        //    return forecast ?? new SimulatedMetricDTO();
        //}

        public async Task<SimulatedMetricDTO> SimulateProjectMetricsAfterRecommendationsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var metric = await _projectMetricRepo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode);
            var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);
            var approvedRecommendtions = await _projectRecommendationRepo.GetByProjectIdAsync(project.Id);

            if (metric == null)
                throw new Exception("ProjectMetric not found");

            var forecast = await _geminiService.SimulateProjectMetricsAfterRecommendationsAsync(
                project,
                metric,
                tasks,
                sprints,
                milestones,
                subtasks,
                approvedRecommendtions
            );

            if (forecast != null)
            {
                var calculationAIMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "AI");
                var existingAIMetric = await _projectMetricRepo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationAIMode);

                if (existingAIMetric != null)
                {
                    existingAIMetric.SchedulePerformanceIndex = Math.Round((decimal)forecast.SchedulePerformanceIndex, 2);
                    existingAIMetric.CostPerformanceIndex = Math.Round((decimal)forecast.CostPerformanceIndex, 2);
                    existingAIMetric.EstimateAtCompletion = Math.Round((decimal)forecast.EstimateAtCompletion, 0);
                    existingAIMetric.EstimateToComplete = Math.Round((decimal)forecast.EstimateToComplete, 0);
                    existingAIMetric.VarianceAtCompletion = Math.Round((decimal)forecast.VarianceAtCompletion, 0);
                    existingAIMetric.EstimateDurationAtCompletion = Math.Round((decimal)forecast.EstimatedDurationAtCompletion, 0);
                    existingAIMetric.UpdatedAt = DateTime.UtcNow;
                    existingAIMetric.IsImproved = forecast.IsImproved;
                    existingAIMetric.ImprovementSummary = forecast.ImprovementSummary;
                    existingAIMetric.ConfidenceScore = forecast.ConfidenceScore;

                    await _projectMetricRepo.Update(existingAIMetric);
                }
                else
                {
                    var newMetric = new ProjectMetric
                    {
                        ProjectId = project.Id,
                        CalculatedBy = calculationAIMode,
                        IsApproved = false,
                        SchedulePerformanceIndex = Math.Round((decimal)forecast.SchedulePerformanceIndex, 2),
                        CostPerformanceIndex = Math.Round((decimal)forecast.CostPerformanceIndex, 2),
                        EstimateAtCompletion = Math.Round((decimal)forecast.EstimateAtCompletion, 0),
                        EstimateToComplete = Math.Round((decimal)forecast.EstimateToComplete, 0),
                        VarianceAtCompletion = Math.Round((decimal)forecast.VarianceAtCompletion, 0),
                        EstimateDurationAtCompletion = Math.Round((decimal)forecast.EstimatedDurationAtCompletion, 0),
                        IsImproved = forecast.IsImproved,
                        ImprovementSummary = forecast.ImprovementSummary,
                        ConfidenceScore = forecast.ConfidenceScore,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _projectMetricRepo.Add(newMetric);
                }

                return forecast;
            }

            return new SimulatedMetricDTO();
        }

        public async Task<SimulatedMetricDTO> GetProjectMetricForecastAsync(string projectKey)
        {
            // Validate projectKey
            if (string.IsNullOrWhiteSpace(projectKey))
                throw new ArgumentException("Project key cannot be empty.", nameof(projectKey));

            // Fetch project data
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception($"Project with key {projectKey} not found");

            // Fetch related data
            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
            var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);
            var calculationMode = await _dynamicCategoryHelper.GetCategoryNameAsync("calculation_mode", "SYSTEM");
            var metric = await _projectMetricRepo.GetByProjectIdAndCalculatedByAsync(project.Id, calculationMode)
                ?? throw new Exception($"ProjectMetric for project {projectKey} not found");
            var approvedRecommendations = await _projectRecommendationRepo.GetByProjectIdAsync(project.Id);

            // Generate forecast using Gemini service
            var forecast = await _geminiService.SimulateProjectMetricsAfterRecommendationsAsync(
                project,
                metric,
                tasks,
                sprints,
                milestones,
                subtasks,
                approvedRecommendations
            );

            if (forecast == null)
                throw new Exception("Failed to generate forecast.");

            return forecast;
        }
    }
}
