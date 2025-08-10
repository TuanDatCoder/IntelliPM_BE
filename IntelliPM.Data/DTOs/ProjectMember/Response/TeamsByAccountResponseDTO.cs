using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class TeamsByAccountResponseDTO
    {
        public int TotalTeams { get; set; }
        public List<TeamWithMembersResponseDTO> Teams { get; set; }
    }
}
