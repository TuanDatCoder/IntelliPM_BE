using IntelliPM.Data.DTOs.ProjectPosition.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class ProjectMemberResponseDTO
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string? AccountEmail { get; set; }
        public string? AccountPicture { get; set; }
        public int ProjectId { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime InvitedAt { get; set; }
        public string? Status { get; set; }
        public string? AccountRole { get; set; }

    }
}
