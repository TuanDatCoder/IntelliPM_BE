using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskResponseDTO
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public int ProjectId { get; set; }
        public int? EpicId { get; set; }
        public int? SprintId { get; set; }
        public int? MilestoneId { get; set; }
        public string? Type { get; set; }
        public bool ManualInput { get; set; }
        public bool GenerationAiInput { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public string? Duration { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal? PlannedCost { get; set; }
        public decimal? PlannedResourceCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? ActualResourceCost { get; set; }
        public decimal? RemainingHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Priority { get; set; }
        public string? Evaluate { get; set; }
        public string? Status { get; set; }
    }
}
