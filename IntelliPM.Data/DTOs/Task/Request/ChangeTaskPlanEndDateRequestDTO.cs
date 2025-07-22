using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskPlanEndDateRequestDTO
    {
        public DateTime PlannedEndDate { get; set; }
        public int CreatedBy { get; set; }
    }
}
