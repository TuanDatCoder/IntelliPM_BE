using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Subtask.Request
{
    public class SubtaskRequest2DTO
    {
        public string TaskId { get; set; } = null!;

        [Required(ErrorMessage = "Subtask title is required")]
        [DynamicMaxLength("title_length")]
        [DynamicMinLength("title_length")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
        public int? ReporterId { get; set; }
    }
}
