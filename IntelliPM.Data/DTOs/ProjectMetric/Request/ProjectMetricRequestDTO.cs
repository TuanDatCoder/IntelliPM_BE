using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Request
{
    public class ProjectMetricRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        [MaxLength(100, ErrorMessage = "CalculatedBy cannot exceed 100 characters")]
        public string? CalculatedBy { get; set; }

        public bool? IsApproved { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Planned Value must be between 0 and 9999999999999.99")]
        public decimal? PlannedValue { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Earned Value must be between 0 and 9999999999999.99")]
        public decimal? EarnedValue { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Actual Cost must be between 0 and 9999999999999.99")]
        public decimal? ActualCost { get; set; }

        public double? SPI { get; set; }   // Schedule Performance Index

        public double? CPI { get; set; }   // Cost Performance Index

        public int? DelayDays { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Budget Overrun must be between 0 and 9999999999999.99")]
        public decimal? BudgetOverrun { get; set; }

        public DateTime? ProjectedFinishDate { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Project Total Cost must be between 0 and 9999999999999.99")]
        public decimal? ProjectTotalCost { get; set; }

        public List<RecommendationSuggestionDTO>? Suggestions { get; set; }
    }

    public class RecommendationSuggestionDTO
    {
        public string Message { get; set; } = null!;
        public string? Reason { get; set; }
        public List<RelatedTaskDTO> RelatedTasks { get; set; } = new();
    }

    public class RelatedTaskDTO
    {
        public string TaskTitle { get; set; } = null!;
        public string? CurrentPlannedEndDate { get; set; }
        public double? CurrentPercentComplete { get; set; }
        public string? SuggestedAction { get; set; }
    }
}
