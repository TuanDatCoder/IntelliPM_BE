using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectPosition.Response
{
    public class ProjectPositionResponseDTO
    {
        public int Id { get; set; }
        public int ProjectMemberId { get; set; }
        public string Position { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
    }
}
