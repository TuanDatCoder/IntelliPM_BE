using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Request
{
    public class RiskWithSolutionDTO
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Probability { get; set; }
        public string? ImpactLevel { get; set; }
        public string? SeverityLevel { get; set; }
        public string? MitigationPlan { get; set; }
        public string? ContingencyPlan { get; set; }
    }

}
