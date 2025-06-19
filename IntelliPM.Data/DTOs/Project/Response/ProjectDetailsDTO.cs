using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.Requirement.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class ProjectDetailsDTO
    {
        public int Id { get; set; }
        public string ProjectKey { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Budget { get; set; }
        public string ProjectType { get; set; } = null!;
        public int CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Status { get; set; }
        public List<RequirementResponseDTO> Requirements { get; set; } = new List<RequirementResponseDTO>();
        public List<ProjectMemberWithPositionsResponseDTO> ProjectMembers { get; set; } = new List<ProjectMemberWithPositionsResponseDTO>();
    }
}
