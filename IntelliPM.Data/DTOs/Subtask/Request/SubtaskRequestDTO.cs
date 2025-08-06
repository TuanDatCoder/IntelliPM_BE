using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskCheckList.Request
{
    public class SubtaskRequestDTO
    {
        public int? AssignedBy { get; set; }
        public string? Priority { get; set; }
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ReporterId { get; set; }
        public int CreatedBy { get; set; }
    }
}
