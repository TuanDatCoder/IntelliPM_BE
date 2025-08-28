using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Request
{
    public class RiskCreateRequestDTO
    {
        [Required(ErrorMessage = "Project key is required")]
        [DynamicMaxLength("project_key_length")]
        [DynamicMinLength("project_key_length")]
        public string ProjectKey { get; set; } = null!;

        public int? ResponsibleId { get; set; }

        [Required(ErrorMessage = "Created by is required")]
        public int? CreatedBy { get; set; }

        public string? TaskId { get; set; }

        [Required(ErrorMessage = "Risk scope is required")]
        [DynamicCategoryValidation("risk_scope", Required = true)]
        public string RiskScope { get; set; } = null!;

        [Required(ErrorMessage = "Risk title is required")]
        [DynamicMaxLength("risk_title_length")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [DynamicCategoryValidation("risk_status")]
        public string? Status { get; set; }

        [DynamicCategoryValidation("risk_type")]
        public string? Type { get; set; }

        [DynamicCategoryValidation("risk_generated_by")]
        public string? GeneratedBy { get; set; }

        [DynamicCategoryValidation("risk_probability_level")]
        public string? Probability { get; set; }

        [DynamicCategoryValidation("risk_impact_level")]
        public string? ImpactLevel { get; set; }

        public bool IsApproved { get; set; }

        [FutureOrTodayDate(ErrorMessage = "Due date cannot be in the past")]
        public DateTime? DueDate { get; set; }
    }

    public class FutureOrTodayDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Optional field
            if (value is DateTime date)
            {
                return date.Date >= DateTime.UtcNow.Date;
            }
            return false;
        }
    }
}
