using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Request
{
    public class ProjectRejectionRequestDTO
    {
        [DynamicMaxLength("reason_length")]
        public string Reason { get; set; } = string.Empty;
    }
}
