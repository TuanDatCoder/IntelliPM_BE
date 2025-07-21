using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class NewProjectMetricResponseDTO
    {
        public int ProjectId { get; set; }
        public decimal PlannedValue { get; set; }
        public decimal EarnedValue { get; set; }
        public decimal ActualCost { get; set; }
        public decimal BudgetAtCompletion { get; set; }
        public decimal DurationAtCompletion { get; set; }
        public decimal CostVariance { get; set; }
        public decimal ScheduleVariance { get; set; }
        public decimal CostPerformanceIndex { get; set; }
        public decimal SchedulePerformanceIndex { get; set; }
        public decimal EstimateAtCompletion { get; set; }
        public decimal EstimateToComplete { get; set; }
        public decimal VarianceAtCompletion { get; set; }
        public decimal EstimateDurationAtCompletion { get; set; }
        public string CalculatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
