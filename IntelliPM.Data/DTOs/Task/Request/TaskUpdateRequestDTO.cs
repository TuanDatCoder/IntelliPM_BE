using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class TaskUpdateRequestDTO
    {
        [Required(ErrorMessage = "Reporter ID is required")]
        public int ReporterId { get; set; }

        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        public string? EpicId { get; set; }

        public int? SprintId { get; set; }

        [DynamicCategoryValidation("task_type", Required = false)]
        public string? Type { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(255, ErrorMessage = "Task title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]

        [DynamicCategoryValidation("task_status", Required = false)]
        public string? Status { get; set; }

    }
}
