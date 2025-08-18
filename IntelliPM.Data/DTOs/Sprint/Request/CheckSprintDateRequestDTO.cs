using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class CheckSprintDateRequestDTO
    {
        [DynamicMaxLength("project_key_length")]
        [DynamicMinLength("project_key_length")]
        public string ProjectKey { get; set; } = null!;
        public DateTime CheckDate { get; set; }
    }
}
