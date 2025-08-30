using IntelliPM.Common.Attributes;
using IntelliPM.Data.DTOs.TaskDependency.Request;
using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class TaskRequestDTO
    {
        [Required(ErrorMessage = "Reporter ID is required")]
        public int ReporterId { get; set; }

        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        public string? EpicId { get; set; }

        public int? SprintId { get; set; }

        [Required]
        [DynamicCategoryValidation("task_type", Required = false)]
        public string? Type { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [DynamicMaxLength("title_length")]
        public string Title { get; set; } = null!;

        [DynamicMaxLength("description_length")]
        public string? Description { get; set; }

        [DynamicCategoryValidation("task_priority", Required = false)]
        public string? Priority { get; set; }
        public decimal? PlannedHours { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [DynamicCategoryValidation("task_status", Required = false)]
        public string? Status { get; set; }
        public int CreatedBy { get; set; }
        public List<TaskDependencyRequestDTO>? Dependencies { get; set; }
    }
}