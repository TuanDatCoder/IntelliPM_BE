using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class MoveTasksToSprintRequestDTO
    {
        [Required]
        public int SprintOldId { get; set; }

        [Required]
        public int SprintNewId { get; set; }

        [DefaultValue("BACKLOG")]
        public string Type { get; set; } = "BACKLOG";
    }
}
