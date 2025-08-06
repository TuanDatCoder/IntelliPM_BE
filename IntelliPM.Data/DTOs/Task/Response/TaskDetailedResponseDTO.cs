using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskDetailedResponseDTO
    {
        public string Id { get; set; } = null!;
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string Type { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int ReporterId { get; set; }
        public string? Priority { get; set; }
        public string? Evaluate { get; set; }
        public decimal? PercentComplete { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public decimal? PlannedCost { get; set; }
        public decimal? PlannedResourceCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? ActualResourceCost { get; set; }
        public decimal? RemainingHours { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public string? EpicId { get; set; }

        // Thông tin Reporter
        public string? ReporterFullname { get; set; }
        public string? ReporterPicture { get; set; }

        // Task Assignment thông tin
        public List<TaskAssignmentResponseDTO> TaskAssignments { get; set; } = new List<TaskAssignmentResponseDTO>();

        // Comment thông tin
        public int CommentCount { get; set; }
        public List<TaskCommentResponseDTO> Comments { get; set; } = new List<TaskCommentResponseDTO>();

        // Label thông tin
        public List<LabelResponseDTO> Labels { get; set; } = new List<LabelResponseDTO>();
    }
}
