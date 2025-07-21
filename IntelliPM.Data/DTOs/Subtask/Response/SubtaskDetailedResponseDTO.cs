using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Response
{
    public class SubtaskDetailedResponseDTO
    {
        public string Id { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public string Type { get; set; } = "SUBTASK";
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int? ReporterId { get; set; }
        public int? AssignedBy { get; set; }
        public string? Priority { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public string? ReporterFullname { get; set; }
        public string? ReporterPicture { get; set; }
        public string? AssignedByFullname { get; set; }
        public string? AssignedByPicture { get; set; }
        public int CommentCount { get; set; }
        public List<SubtaskCommentResponseDTO> Comments { get; set; } = new List<SubtaskCommentResponseDTO>();

        public List<LabelResponseDTO> Labels { get; set; } = new List<LabelResponseDTO>();
    }
}
