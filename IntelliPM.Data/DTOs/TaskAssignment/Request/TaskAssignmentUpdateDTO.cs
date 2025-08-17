using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskAssignment.Request
{
    public class TaskAssignmentUpdateDTO
    {
        public int AssignmentId { get; set; }

        public decimal PlannedHours { get; set; }
    }
}
