using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Request
{
    public class SubtaskRequest3DTO
    {
        public string? Status { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; } 
    }
}
