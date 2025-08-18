using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class SprintRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Sprint name is required")]
        [DynamicMaxLength("sprint_name_length")]
        public string Name { get; set; } = null!;

        public string? Goal { get; set; }

        [DynamicDuration("sprint_duration_days")]
        public DateTime? StartDate { get; set; }

        [DynamicDuration("sprint_duration_days")]
        public DateTime? EndDate { get; set; }

        [DynamicDuration("sprint_planned_duration_days")]
        public DateTime? PlannedStartDate { get; set; }

        [DynamicDuration("sprint_planned_duration_days")]
        public DateTime? PlannedEndDate { get; set; }


        [DynamicCategoryValidation("sprint_status", Required = false)]
        [DynamicMaxLength("sprint_status_length")]
        public string? Status { get; set; }
    }
}
