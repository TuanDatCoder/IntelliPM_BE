using IntelliPM.Common.Attributes;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskCheckList.Request
{
    public class SubtaskRequestDTO
    {
        public int? AssignedBy { get; set; }
        public int? SprintId { get; set; }

        [DynamicCategoryValidation("subtask_priority", Required = false)]
        public string? Priority { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(65, ErrorMessage = "Task title cannot exceed 65 characters")]
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ReporterId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
