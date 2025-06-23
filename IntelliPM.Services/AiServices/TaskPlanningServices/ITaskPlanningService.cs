using IntelliPM.Data.DTOs.Ai.ProjectTaskPlanning.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.TaskPlanningServices
{
    public interface ITaskPlanningService
    {
        Task<List<object>> GenerateTaskPlan(int projectId);
    }
}
