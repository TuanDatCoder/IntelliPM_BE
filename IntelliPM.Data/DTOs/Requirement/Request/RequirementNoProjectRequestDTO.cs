using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Requirement.Request
{
    public class RequirementNoProjectRequestDTO
    {


        [Required(ErrorMessage = "Requirement title is required")]
        [DynamicMaxLength("title_length")]
        public string Title { get; set; } = null!;

        [DynamicMaxLength("type_length")]
        [DynamicCategoryValidation("requirement_type", Required = false)]
        public string? Type { get; set; }

        [DynamicMaxLength("description_length")]
        public string? Description { get; set; }

        [DynamicMaxLength("priority_length")]
        [DynamicCategoryValidation("requirement_priority", Required = false)]
        public string? Priority { get; set; }

    }
}
