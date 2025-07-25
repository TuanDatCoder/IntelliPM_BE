﻿using IntelliPM.Data.DTOs.TaskDependency.Request;
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
        [RegularExpression("BUG|TASK|STORY", ErrorMessage = "Type must be BUG, TASK, or STORY")]
        public string? Type { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(255, ErrorMessage = "Task title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        public List<TaskDependencyRequestDTO>? Dependencies { get; set; }
    }
}