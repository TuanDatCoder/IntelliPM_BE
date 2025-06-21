using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class ProjectMetricResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? CalculatedBy { get; set; }
        public bool? IsApproved { get; set; }
        public decimal? PlannedValue { get; set; }
        public decimal? EarnedValue { get; set; }
        public decimal? ActualCost { get; set; }
        public double? SPI { get; set; }
        public double? CPI { get; set; }
        public int? DelayDays { get; set; }
        public decimal? BudgetOverrun { get; set; }
        public DateTime? ProjectedFinishDate { get; set; }
        public decimal? ProjectTotalCost { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
