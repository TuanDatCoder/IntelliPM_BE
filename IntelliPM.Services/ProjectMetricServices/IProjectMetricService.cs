using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
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
        Task<List<ProjectMetricResponseDTO>> GetByProjectIdAsync(int projectId);
        Task<ProjectHealthDTO> GetProjectHealthAsync(int projectId);
        Task<ProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(int projectId, string calculatedBy);
        Task<ProjectMetricRequestDTO> CalculateMetricsByAIAsync(int projectId);
        Task<object> GetTaskStatusDashboardAsync(int projectId);
        Task<List<object>> GetProgressDashboardAsync(int projectId);
        Task<object> GetTimeDashboardAsync(int projectId);
        Task<CostDashboardResponseDTO> GetCostDashboardAsync(int projectId);
        Task<List<WorkloadDashboardResponseDTO>> GetWorkloadDashboardAsync(int projectId);
    }
}
