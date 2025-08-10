using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectRecommendation.Response
{
    public class ProjectRecommendationResponseDTO
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public string Type { get; set; } = null!;

        public string Recommendation { get; set; } = null!;

        public string? Details { get; set; }

        public string? SuggestedChanges { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
