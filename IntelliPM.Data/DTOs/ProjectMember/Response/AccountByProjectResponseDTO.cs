using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class AccountByProjectResponseDTO
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
    }
}
