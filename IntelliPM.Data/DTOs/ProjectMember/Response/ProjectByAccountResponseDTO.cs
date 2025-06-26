using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class ProjectByAccountResponseDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string ProjectStatus { get; set; } = null!;
        public DateTime? JoinedAt { get; set; }
        public DateTime InvitedAt { get; set; }
        public string? Status { get; set; }
    }
}
