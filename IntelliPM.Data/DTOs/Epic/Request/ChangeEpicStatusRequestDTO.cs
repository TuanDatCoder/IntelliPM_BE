using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class ChangeEpicStatusRequestDTO
    {
        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
        public string? Status { get; set; }
    }
}
