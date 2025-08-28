using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Response
{
    public class RiskStatisticsDTO
    {
        public int TotalRisks { get; set; }
        public int OverdueRisks { get; set; }
        public Dictionary<string, int> RisksByStatus { get; set; } = new();
        public Dictionary<string, int> RisksByType { get; set; } = new();
        public Dictionary<string, int> RisksBySeverity { get; set; } = new();
        public Dictionary<string, int> RisksByResponsible { get; set; } = new();
    }
}
