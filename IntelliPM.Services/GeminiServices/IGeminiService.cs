using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.GeminiServices
{
    public interface IGeminiService
    {
        Task<List<string>> GenerateSubtaskAsync(string taskTitle);
        Task<ProjectMetricRequestDTO> CalculateProjectMetricsAsync(Project project, List<Tasks> tasks);
        Task<string> SummarizeTextAsync(string transcriptText);
        Task<List<RiskRequestDTO>> DetectProjectRisksAsync(Project project, List<Tasks> tasks);
        Task<List<AIRecommendationDTO>> GenerateProjectRecommendationsAsync(Project project, ProjectMetric metric, List<Tasks> tasks, List<Sprint> sprints, List<Milestone> milestones, List<Subtask> subtasks);
        Task<List<AIRiskResponseDTO>> ViewAIProjectRisksAsync(Project project, List<Tasks> tasks);
        Task<SimulatedMetricDTO> SimulateProjectMetricsAfterRecommendationsAsync(Project project, ProjectMetric currentMetric, List<Tasks> tasks, List<Sprint> sprints, List<Milestone> milestones, List<Subtask> subtasks, List<ProjectRecommendation> approvedRecommendations);
    }
}
