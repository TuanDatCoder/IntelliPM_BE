using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RiskSolution.Request
{
    public class RiskSolutionRequestDTO
    {
        public int RiskId { get; set; }

        public string? MitigationPlan { get; set; }

        public string? ContingencyPlan { get; set; }
    }

}
