using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectRecommendation.Request
{
    public class ProjectRecommendationRequestDTO
    {
        public int ProjectId { get; set; }

        public string Type { get; set; } = null!;

        public string Recommendation { get; set; } = null!;

        public string SuggestedChanges { get; set; }

        public string Details { get; set; }
    }

}
