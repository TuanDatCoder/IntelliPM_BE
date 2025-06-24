using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Request
{
    public class SubtaskRequest1DTO
    {
        public string TaskId { get; set; } = null!;

        public int AssignedBy { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }
    }
}
