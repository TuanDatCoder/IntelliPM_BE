using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Request
{
    public class ProjectRequestDTO
    {
        [Required(ErrorMessage = "Project name is required")]
        [MaxLength(255, ErrorMessage = "Project name cannot exceed 255 characters")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, 9999999999999.99, ErrorMessage = "Budget must be between 0 and 9999999999999.99")]
        public decimal? Budget { get; set; }

        [Required(ErrorMessage = "Project type is required")]
        [MaxLength(50, ErrorMessage = "Project type cannot exceed 50 characters")]
        public string ProjectType { get; set; } = null!;

        [Required(ErrorMessage = "Created by is required")]
        public int CreatedBy { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }
    }
}
