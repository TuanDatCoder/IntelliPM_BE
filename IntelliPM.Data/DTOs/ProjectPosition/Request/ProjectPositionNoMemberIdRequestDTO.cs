using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectPosition.Request
{
    public class ProjectPositionNoMemberIdRequestDTO
    {
        [Required(ErrorMessage = "Position is required")]
        [MaxLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        public string Position { get; set; } = null!;

    }
}
