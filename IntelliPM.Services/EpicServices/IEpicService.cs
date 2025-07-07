using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicServices
{
    public interface IEpicService
    {
        Task<List<EpicResponseDTO>> GetAllEpics();
        Task<EpicResponseDTO> GetEpicById(string id);
        Task<List<EpicResponseDTO>> GetEpicByName(string name);
        Task<EpicDetailedResponseDTO> GetEpicByIdDetailed(string id);
        Task<List<EpicDetailedResponseDTO>> GetEpicsByProjectId(int projectId);
        Task<EpicResponseDTO> CreateEpic(EpicRequestDTO request);
        Task<EpicResponseDTO> UpdateEpic(string id, EpicRequestDTO request);
        Task DeleteEpic(string id);
        Task<EpicResponseDTO> ChangeEpicStatus(string id, string status);
        Task<string> CreateEpicWithTaskAndAssignment(int projectId, string token, EpicWithTaskRequestDTO request);
    }
}
