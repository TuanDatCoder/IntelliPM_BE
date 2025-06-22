using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Milestone.Request
{
    public class MilestoneRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }
        public int? SprintId { get; set; }

        [Required(ErrorMessage = "Milestone name is required")]
        [MaxLength(255, ErrorMessage = "Milestone name cannot exceed 255 characters")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; } 

        public DateTime? EndDate { get; set; } 

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
