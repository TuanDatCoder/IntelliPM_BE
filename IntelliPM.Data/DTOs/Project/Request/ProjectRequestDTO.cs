using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliPM.Common.Attributes;


namespace IntelliPM.Data.DTOs.Project.Request
{
    public class ProjectRequestDTO
    {
        [Required(ErrorMessage = "Project name is required")]
        [DynamicMaxLength("project_name_length")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Project key is required")]
        [DynamicMaxLength("project_key_length")]
        [DynamicMinLength("project_key_length")]
        public string ProjectKey { get; set; } = null!;

        [DynamicMaxLength("description_length")]
        public string? Description { get; set; }

        [DynamicRange("project_budget")]
        public decimal? Budget { get; set; }

        [Required(ErrorMessage = "Project type is required")]
        [DynamicCategoryValidation("project_type", Required = true)]
        public string ProjectType { get; set; } = null!;

        [DynamicDuration("project_duration_days")]
        public DateTime? StartDate { get; set; }

        [DynamicDuration("project_duration_days")]
        public DateTime? EndDate { get; set; }

    }
}
