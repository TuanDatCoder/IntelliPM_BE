using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request
{
    public class ProjectTaskPlanningRequestDTO
    {
        [Required(ErrorMessage = "Project ID is required")]
        public int ProjectId { get; set; }

    }
}
