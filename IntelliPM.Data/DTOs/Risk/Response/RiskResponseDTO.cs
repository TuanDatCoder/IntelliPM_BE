using IntelliPM.Data.DTOs.Account.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Risk.Response
{
    public class RiskResponseDTO
    {
        public int Id { get; set; }

        public string RiskKey { get; set; } = null!;

        public int CreatedBy { get; set; }

        public string CreatorFullName { get; set; } = null!;

        public string CreatorUserName { get; set; } = null!;

        public string? CreatorPicture { get; set; }

        public int ResponsibleId { get; set; }

        public string ResponsibleFullName { get; set; } = null!;

        public string ResponsibleUserName { get; set; } = null!;

        public string? ResponsiblePicture { get; set; }

        public int ProjectId { get; set; }

        public string? TaskId { get; set; }

        public string? TaskTitle { get; set; }

        public string RiskScope { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }

        public string? GeneratedBy { get; set; }

        public string? Probability { get; set; }

        public string? ImpactLevel { get; set; }

        public string? SeverityLevel { get; set; }

        public bool IsApproved { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
