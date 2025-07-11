using AutoMapper;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.ProjectRecommendationRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
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
        private readonly ILogger<ProjectRecommendationService> _logger;
        private readonly IGeminiService _geminiService;

        public ProjectRecommendationService(IMapper mapper, IProjectMetricRepository projectMetricRepo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectRecommendationService> logger, IGeminiService geminiService, ISprintRepository sprintRepo, IMilestoneRepository milestoneRepo, IProjectRecommendationRepository projectRecommendationRepo)
        {
            _mapper = mapper;
            _projectMetricRepo = projectMetricRepo;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _sprintRepo = sprintRepo;
            _milestoneRepo = milestoneRepo;
            _projectRecommendationRepo = projectRecommendationRepo;
            _logger = logger;
            _geminiService = geminiService;
        }

        public async Task<List<AIRecommendationDTO>> GenerateProjectRecommendationsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey)
                ?? throw new Exception("Project not found");

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
            var metric = await _projectMetricRepo.GetByProjectIdAsync(project.Id);

            if (metric == null)
                throw new Exception("ProjectMetric not found");

            // Nếu dự án không gặp vấn đề về SPI hoặc CPI thì không cần gọi AI
            if (metric.Spi >= 1 && metric.Cpi >= 1)
                return new List<AIRecommendationDTO>();

            // Gọi AI sinh recommendations
            var recommendations = await _geminiService.GenerateProjectRecommendationsAsync(
                project,
                metric,
                tasks,
                sprints,
                milestones
            );

            return recommendations ?? new List<AIRecommendationDTO>();
        }

    }
}
