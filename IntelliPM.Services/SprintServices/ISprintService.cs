using IntelliPM.Data.DTOs.Sprint.Request;
using IntelliPM.Data.DTOs.Sprint.Response;
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
        Task<SprintResponseDTO> ChangeSprintStatus(int id, string status);
        Task<List<SprintResponseDTO>> GetSprintByProjectId(int projectId);
    }
}
