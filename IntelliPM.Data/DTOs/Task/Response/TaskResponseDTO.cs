using System;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskResponseDTO
    {
        public string Id { get; set; } = null!;

        public int ReporterId { get; set; }

        public int ProjectId { get; set; }

        public string? EpicId { get; set; }

        public int? SprintId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}