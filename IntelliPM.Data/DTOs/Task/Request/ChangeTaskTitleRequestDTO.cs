using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskTitleRequestDTO
    {
        [Required(ErrorMessage = "Task title is required")]
        [DynamicMaxLength("title_length")]
        [DynamicMinLength("title_length")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
