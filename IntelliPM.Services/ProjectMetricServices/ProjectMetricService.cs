using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.TaskRepos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMetricServices
{
    public class ProjectMetricService : IProjectMetricService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMetricRepository _repo;
        private readonly ITaskRepository _taskRepo;
        private readonly ILogger<ProjectMetricService> _logger;

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _taskRepo = taskRepo;
            _logger = logger;
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

        public async Task<List<ProjectMetricResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            var entities = await _repo.GetByProjectIdAsync(projectId);
            return _mapper.Map<List<ProjectMetricResponseDTO>>(entities);
        }

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(int projectId)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var allMetrics = await _repo.GetAllAsync();
            var latestMetric = allMetrics
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

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
    }
}
