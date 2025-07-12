using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectRecommendation.Response
{
    public class AIRecommendationDTO
    {
        //public string Recommendation { get; set; }
        //public string Reason { get; set; }
        //public string Type { get; set; }
        public string Recommendation { get; set; }
        public string Details { get; set; }
        public string Type { get; set; }
        public List<string> AffectedTasks { get; set; }
        public string SuggestedTask { get; set; }
        public string ExpectedImpact { get; set; }
        public Dictionary<string, object> SuggestedChanges { get; set; }
    }

}
