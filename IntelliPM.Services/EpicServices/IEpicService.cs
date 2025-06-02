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
        Task<EpicResponseDTO> GetEpicById(int id);
        Task<List<EpicResponseDTO>> GetEpicByName(string name);
        Task<EpicResponseDTO> CreateEpic(EpicRequestDTO request);
        Task<EpicResponseDTO> UpdateEpic(int id, EpicRequestDTO request);
        Task DeleteEpic(int id);
        Task<EpicResponseDTO> ChangeEpicStatus(int id, string status);
    }
}
