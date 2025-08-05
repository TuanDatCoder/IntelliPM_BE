using IntelliPM.Data.DTOs.ProjectMetric.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.External.ProjectMetricApi
{
    public class ProjectMetricApiResponse
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public ProjectMetricResponseDTO? Data { get; set; }
        public string? Message { get; set; }
    }
}
