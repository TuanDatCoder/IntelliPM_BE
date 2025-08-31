using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskCheckList.Response
{
    public class SubtaskResponseDTO
    {
        public string Id { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public int AssignedBy { get; set; }
        public string? AssignedByName { get; set; }
        public string? AssignedByPicture { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public int? ReporterId { get; set; }
        public string? ReporterName { get; set; }
        public string? ReporterPicture { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public bool ManualInput { get; set; }
        public bool GenerationAiInput { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Warnings { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal ActualCost { get; set; }
    }
}
