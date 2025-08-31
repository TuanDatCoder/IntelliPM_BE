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

        [Required(ErrorMessage = "Subtask title is required")]
        [DynamicMaxLength("title_length")]
        [DynamicMinLength("title_length")]
        public string? Title { get; set; } = null!;

        //[DynamicMaxLength("description_length")]
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ReporterId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
