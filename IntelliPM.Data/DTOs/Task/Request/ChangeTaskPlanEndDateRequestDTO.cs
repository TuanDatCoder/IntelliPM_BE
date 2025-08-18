using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskPlanEndDateRequestDTO
    {
        public DateTime PlannedEndDate { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
