using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicWithTaskTypeRequestDTO
    {
        [DynamicMaxLength("title_length")]
        public string Title { get; set; }

        [DynamicMaxLength("description_length")]
        public string Description { get; set; }

        [DynamicDuration("epic_duration_days")]
        public DateTime StartDate { get; set; }
        [DynamicDuration("epic_duration_days")]
        public DateTime EndDate { get; set; }

        public List<EpicTaskTypeAssignedMembersRequestDTO> Tasks { get; set; } = new List<EpicTaskTypeAssignedMembersRequestDTO>();

    }
}
