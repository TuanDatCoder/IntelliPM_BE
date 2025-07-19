using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Response
{
    public class SubtaskBasicDTO
    {
        public string Id { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public int AssignedBy { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }
}
