using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectPosition.Request
{
    public class AssignedPositionRequestDTO
    {
        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Assigned date is required")]
        public DateTime AssignedAt { get; set; }
    }
}
