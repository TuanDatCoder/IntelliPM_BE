using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskPriorityRequestDTO
    {
        [DynamicCategoryValidation("task_priority", Required = false)]
        public string? Priority { get; set; }
        public int CreatedBy { get; set; }
    }
}
