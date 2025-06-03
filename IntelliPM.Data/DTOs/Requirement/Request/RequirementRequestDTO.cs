using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Requirement.Request
{
    public class RequirementRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Requirement title is required")]
        [MaxLength(255, ErrorMessage = "Requirement title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string? Type { get; set; }

        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
        public string? Priority { get; set; }
    }
}
