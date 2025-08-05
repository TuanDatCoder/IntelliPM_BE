using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskDependency.Response;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskBacklogResponseDTO
    {
        public string Id { get; set; } = null!;
        public int ReporterId { get; set; }
        public string? ReporterName { get; set; }
        public string? ReporterPicture { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? EpicId { get; set; }
        public string? EpicName { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public string? Type { get; set; }
        public bool ManualInput { get; set; }
        public bool GenerationAiInput { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal? PlannedHours { get; set; }
        public string? Duration { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TaskAssignmentResponseDTO> TaskAssignments { get; set; } = new List<TaskAssignmentResponseDTO>();
    }
}