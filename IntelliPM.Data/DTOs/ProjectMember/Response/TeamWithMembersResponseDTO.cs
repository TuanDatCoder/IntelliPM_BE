using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class TeamWithMembersResponseDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public int TotalMembers { get; set; }
        public List<ProjectMemberResponseDTO> Members { get; set; }
    }
}
