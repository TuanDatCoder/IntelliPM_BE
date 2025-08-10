using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetricHistory.Response
{
    public class ProjectMetricHistoryResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string MetricKey { get; set; }
        public string Value { get; set; } // JSON string
        public DateTime RecordedAt { get; set; }
    }
}
