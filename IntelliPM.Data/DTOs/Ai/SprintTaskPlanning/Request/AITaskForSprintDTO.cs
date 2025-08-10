using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Ai.SprintTaskPlanning.Request
{
    public class AITaskForSprintDTO
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public decimal PlannedHours { get; set; }
    }
}
