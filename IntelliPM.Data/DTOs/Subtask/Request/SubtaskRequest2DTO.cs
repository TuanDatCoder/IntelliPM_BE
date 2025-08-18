using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Request
{
    public class SubtaskRequest2DTO
    {
        public string TaskId { get; set; } = null!;

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(65, ErrorMessage = "Task title cannot exceed 65 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
