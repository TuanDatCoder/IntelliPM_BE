using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class ProjectResponseDTO
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
        public string? Status { get; set; }
    }
}
