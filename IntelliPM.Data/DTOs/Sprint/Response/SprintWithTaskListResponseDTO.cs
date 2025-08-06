using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Response
{
    public class SprintWithTaskListResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public string? Goal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public List<TaskBacklogResponseDTO> Tasks { get; set; } = new List<TaskBacklogResponseDTO>();
    }
}
