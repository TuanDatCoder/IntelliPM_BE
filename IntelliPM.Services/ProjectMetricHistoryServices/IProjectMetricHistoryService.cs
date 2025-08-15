using IntelliPM.Data.DTOs.ProjectMetricHistory.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMetricHistoryServices
{
    public interface IProjectMetricHistoryService
    {
        Task<List<ProjectMetricHistoryResponseDTO>> GetByProjectKeyAsync(string projectKey);
    }
}
