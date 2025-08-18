using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class SprintQuickRequestDTO
    {
        [DynamicMaxLength("project_key_length")]
        [DynamicMinLength("project_key_length")]
        public string projectKey { get; set; }

    }
}
