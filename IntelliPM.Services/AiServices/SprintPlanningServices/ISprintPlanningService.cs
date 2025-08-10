using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.SprintPlanningServices
{
    public interface ISprintPlanningService
    {
        Task<List<SprintWithTasksDTO>> GenerateSprintPlan(int projectId, int numberOfSprints, int weeksPerSprint);

    }
}
