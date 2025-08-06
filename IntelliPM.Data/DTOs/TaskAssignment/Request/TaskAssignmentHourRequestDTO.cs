using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskAssignment.Request
{
    public class TaskAssignmentHourRequestDTO
    {
        public int Id { get; set; }
        public decimal ActualHours { get; set; }
    }
}
