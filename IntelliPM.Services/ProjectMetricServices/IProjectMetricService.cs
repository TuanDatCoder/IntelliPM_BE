using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMetricServices
{
    public interface IProjectMetricService
    {
        Task<List<ProjectMetricResponseDTO>> GetAllAsync();
        Task<ProjectMetricResponseDTO> GetByIdAsync(int id);
        Task<ProjectMetricResponseDTO?> GetByProjectIdAsync(int projectId);
        Task<ProjectHealthDTO> GetProjectHealthAsync(string projectKey);
        Task<ProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(int projectId, string calculatedBy);
        Task<ProjectMetricRequestDTO> CalculateMetricsByAIAsync(int projectId);
        Task<object> GetTaskStatusDashboardAsync(string projectKey);
        Task<List<object>> GetProgressDashboardAsync(string projectKey);
        Task<object> GetTimeDashboardAsync(string projectKey);
        Task<CostDashboardResponseDTO> GetCostDashboardAsync(string projectKey);
        Task<List<WorkloadDashboardResponseDTO>> GetWorkloadDashboardAsync(string projectKey);
        Task<ProjectMetricRequestDTO> CalculateProjectMetricsByAIAsync(string projectKey);
    }
}
