
using IntelliPM.Data.DTOs.Ai.SprintTaskPlanning.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntelliPM.Services.AiServices.SprintTaskPlanningServices.SprintTaskPlanningService;

namespace IntelliPM.Services.AiServices.SprintTaskPlanningServices
{
    public interface ISprintTaskPlanningService
    {
        Task<List<AITaskForSprintDTO>> GenerateTasksForSprintAsync(int sprintId, string projectKey);
    }
}
