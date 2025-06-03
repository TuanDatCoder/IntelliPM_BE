using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class TaskRequestDTO
    {
        [Required(ErrorMessage = "Reporter ID is required")]
        public int ReporterId { get; set; }

        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        public int? EpicId { get; set; }

        public int? SprintId { get; set; }

        public int? MilestoneId { get; set; }

        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string? Type { get; set; }

        public bool ManualInput { get; set; } = false; // Giá trị mặc định

        public bool GenerationAiInput { get; set; } = false; // Giá trị mặc định

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(255, ErrorMessage = "Task title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        [MaxLength(100, ErrorMessage = "Duration cannot exceed 100 characters")]
        public string? Duration { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        [Range(0, 100, ErrorMessage = "Percent complete must be between 0 and 100")]
        public decimal? PercentComplete { get; set; }

        public decimal? PlannedHours { get; set; }

        public decimal? ActualHours { get; set; }

        public decimal? PlannedCost { get; set; }

        public decimal? PlannedResourceCost { get; set; }

        public decimal? ActualCost { get; set; }

        public decimal? ActualResourceCost { get; set; }

        public decimal? RemainingHours { get; set; }

        [MaxLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string? Priority { get; set; }

        [MaxLength(50, ErrorMessage = "Evaluate cannot exceed 50 characters")]
        public string? Evaluate { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
