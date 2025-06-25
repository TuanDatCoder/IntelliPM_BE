using IntelliPM.Data.DTOs.ProjectMetric.Request;
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
    }

}
