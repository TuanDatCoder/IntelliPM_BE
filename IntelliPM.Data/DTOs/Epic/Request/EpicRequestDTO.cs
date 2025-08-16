using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Epic name is required")]
        [DynamicMaxLength("title_length")]
        public string Name { get; set; } = null!;

        public int? ReporterId { get; set; }

        public int? AssignedBy { get; set; }

        public string? Description { get; set; }

        [DynamicDuration("epic_duration_days")]
        public DateTime? StartDate { get; set; }
        [DynamicDuration("epic_duration_days")]
        public DateTime? EndDate { get; set; }   

       
        [DynamicCategoryValidation("epic_status", Required = false)]
        public string? Status { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }

    }
}
