using IntelliPM.Data.DTOs.TaskDependency.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Milestone.Response
{
    public class MilestoneDependencyResponseDTO
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int ProjectId { get; set; }
        public int? SprintId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public List<TaskDependencyResponseDTO>? Dependencies { get; set; }
    }
}
