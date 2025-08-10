using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Response
{
    public class SubtaskFullResponseDTO
    {
        public string Id { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public int AssignedBy { get; set; }
        public string AssignedFullName { get; set; }
        public string AssignedUsername { get; set; }
        public string AssignedPicture {  get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? ReporterId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public bool ManualInput { get; set; }
        public bool GenerationAiInput { get; set; }
        public int? SprintId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public string? Duration { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal? RemainingHours { get; set; }
        public decimal? PlannedCost { get; set; }
        public decimal? PlannedResourceCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? ActualResourceCost { get; set; }
        public string? Evaluate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
