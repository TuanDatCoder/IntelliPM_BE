using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicWithTaskRequestDTO
    {
        public string Title { get; set; } 
        public string Description { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<EpicTaskAssignedMembersRequestDTO> Tasks { get; set; } = new List<EpicTaskAssignedMembersRequestDTO>();
    }
}
