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
        [MaxLength(255, ErrorMessage = "Epic name cannot exceed 255 characters")]
        public string Name { get; set; } = null!;


        public int? ReporterId { get; set; }

        public int? AssignedBy { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; }   

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        //[DynamicCategoryValidation("epic_status", Required = false)]
        public string? Status { get; set; }
    }
}
