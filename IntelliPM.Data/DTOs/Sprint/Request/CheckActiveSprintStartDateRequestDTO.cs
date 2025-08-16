using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class CheckActiveSprintStartDateRequestDTO
    {

        [DynamicMaxLength("project_key_length")]
        [DynamicMinLength("project_key_length")]
        public string ProjectKey { get; set; }
        public DateTime CheckStartDate { get; set; }
        public DateTime CheckEndDate { get; set; }
        public int ActiveSprintId { get; set; }
    }
}
