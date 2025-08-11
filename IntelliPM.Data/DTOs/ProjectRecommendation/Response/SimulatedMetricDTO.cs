using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectRecommendation.Response
{
    //public class SimulatedMetricDTO
    //{
    //    public double SchedulePerformanceIndex { get; set; }
    //    public double CostPerformanceIndex { get; set; }
    //    public double EstimateAtCompletion { get; set; }   // EAC
    //    public double EstimateToComplete { get; set; }     // ETC
    //    public double VarianceAtCompletion { get; set; }   // VAC
    //    public double EstimatedDurationAtCompletion { get; set; } // EDAC
    //}

    public class SimulatedMetricDTO
    {
        public double SchedulePerformanceIndex { get; set; }
        public double CostPerformanceIndex { get; set; }
        public double EstimateAtCompletion { get; set; }
        public double EstimateToComplete { get; set; }
        public double VarianceAtCompletion { get; set; }
        public double EstimatedDurationAtCompletion { get; set; }
        public bool IsImproved { get; set; }
        public string ImprovementSummary { get; set; } = string.Empty;
        public decimal ConfidenceScore { get; set; }
    }
}
