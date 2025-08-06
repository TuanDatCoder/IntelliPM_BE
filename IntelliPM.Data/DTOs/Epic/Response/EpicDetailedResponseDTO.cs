using IntelliPM.Data.DTOs.EpicComment.Response;
using IntelliPM.Data.DTOs.Label.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Response
{
    public class EpicDetailedResponseDTO
    {
        public string Id { get; set; } = null!;
        public int ProjectId { get; set; }
        public string Type { get; set; } = "EPIC";
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public int? ReporterId { get; set; }
        public int? AssignedBy { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public string? ReporterFullname { get; set; }
        public string? ReporterPicture { get; set; }
        public string? AssignedByFullname { get; set; }
        public string? AssignedByPicture { get; set; }

        public int CommentCount { get; set; }
        public List<EpicCommentResponseDTO> Comments { get; set; } = new List<EpicCommentResponseDTO>();

        // Label thông tin
        public List<LabelResponseDTO> Labels { get; set; } = new List<LabelResponseDTO>();
    }
}

