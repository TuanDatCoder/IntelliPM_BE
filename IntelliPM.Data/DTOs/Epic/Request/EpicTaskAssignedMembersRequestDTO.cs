using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicTaskAssignedMembersRequestDTO
    {
        [DynamicMaxLength("title_length")]
        public string Title { get; set; } 
        public string Description { get; set; }
        [DynamicDuration("epic_duration_days")]
        public DateTime StartDate { get; set; }

        [DynamicDuration("epic_duration_days")]
        public DateTime EndDate { get; set; }
        public string SuggestedRole { get; set; } 
        public List<TaskAssignedMembersRequestDTO> AssignedMembers { get; set; } = new List<TaskAssignedMembersRequestDTO>();
    }
}
