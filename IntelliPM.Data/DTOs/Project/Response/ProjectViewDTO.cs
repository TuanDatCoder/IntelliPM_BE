using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class ProjectViewDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string ProjectKey { get; set; }
        public string? Description { get; set; }
        public decimal? Budget { get; set; }
        public string? ProjectType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? IconUrl { get; set; }
        public string? Status { get; set; }

        public List<SprintResponseDTO> Sprints { get; set; }
        public List<TaskSubtaskDependencyResponseDTO> Tasks { get; set; }
        public List<MilestoneResponseDTO> Milestones { get; set; }
    }
}
