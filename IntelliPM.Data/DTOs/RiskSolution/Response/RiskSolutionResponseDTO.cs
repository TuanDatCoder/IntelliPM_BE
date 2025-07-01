using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RiskSolution.Response
{
    public class RiskSolutionResponseDTO
    {
        public int Id { get; set; }

        public int RiskId { get; set; }

        public string? MitigationPlan { get; set; }

        public string? ContingencyPlan { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
