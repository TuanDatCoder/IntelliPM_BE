using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskTypeRequestDTO
    {
        [DynamicCategoryValidation("task_type", Required = true)]
        public string? Type { get; set; }
        public int CreatedBy { get; set; }
    }
}
