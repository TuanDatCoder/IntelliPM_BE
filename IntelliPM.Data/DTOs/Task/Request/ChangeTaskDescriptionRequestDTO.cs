using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskDescriptionRequestDTO
    {
        [DynamicMaxLength("description_length")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
