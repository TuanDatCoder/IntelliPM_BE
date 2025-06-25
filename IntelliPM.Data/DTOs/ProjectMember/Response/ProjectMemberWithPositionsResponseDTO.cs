using IntelliPM.Data.DTOs.ProjectPosition.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class ProjectMemberWithPositionsResponseDTO
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int ProjectId { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime InvitedAt { get; set; }
        public string? Status { get; set; }
        public string? FullName { get; set; }    
        public string? Username { get; set; } 
        public string? Picture { get; set; }
        public List<ProjectPositionResponseDTO> ProjectPositions { get; set; } = new List<ProjectPositionResponseDTO>();
    }
}
