using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Response
{
    public class AIRiskResponseDTO
    {
        public int ProjectId { get; set; }

        public string? TaskId { get; set; }

        public string RiskScope { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? Type { get; set; }

        public string? Probability { get; set; }

        public string? ImpactLevel { get; set; }

        public string? MitigationPlan { get; set; }

        public string? ContingencyPlan { get; set; }
    }
}
