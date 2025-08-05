using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request
{
    public class SprintPlanningRequestDTO
    {
        public int NumberOfSprints { get; set; }
        public int WeeksPerSprint { get; set; }
    }
}
