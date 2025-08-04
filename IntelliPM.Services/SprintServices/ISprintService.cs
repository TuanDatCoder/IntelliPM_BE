using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Services.AiServices.SprintPlanningServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SprintServices
{
    public interface ISprintService
    {
        Task<List<SprintResponseDTO>> GetAllSprints();
        Task<SprintResponseDTO> GetSprintById(int id);
        Task<List<SprintResponseDTO>> GetSprintByName(string name);
        Task<SprintResponseDTO> CreateSprint(SprintRequestDTO request);
        Task<SprintResponseDTO> UpdateSprint(int id, SprintRequestDTO request);
        Task DeleteSprint(int id);
        Task DeleteSprintWithTask(int id);
        Task<SprintResponseDTO> ChangeSprintStatus(int id, string status);
        Task<List<SprintResponseDTO>> GetSprintByProjectId(int projectId);
        Task<List<SprintResponseDTO>> GetSprintByProjectIdDescending(int projectId);
        Task<List<SprintWithTaskListResponseDTO>> GetSprintsByProjectKeyWithTasksAsync(string projectKey);
        Task<SprintResponseDTO> CreateSprintQuickAsync(SprintQuickRequestDTO request);
        Task<(bool IsValid, string Message)> CheckSprintDatesAsync(string projectKey, DateTime checkStartDate);
        Task<bool> IsSprintWithinProject(string projectKey, DateTime checkSprintDate);
        Task<string> MoveTaskToSprint(int sprintOldId, int sprintNewId, string type);
        Task<SprintResponseDTO> GetActiveSprintWithTasksByProjectKeyAsync(string projectKey);
        Task<List<SprintResponseDTO>> CreateSprintAndAddTaskAsync(string projectKey, List<SprintWithTasksDTO> requests);
        Task<(bool IsValid, string Message)> CheckSprintDatesAsync(string projectKey, DateTime checkStartDate, DateTime checkEndDate);

    }
}
