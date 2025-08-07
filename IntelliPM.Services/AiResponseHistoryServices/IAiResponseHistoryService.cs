using IntelliPM.Data.DTOs.AiResponseHistory.Request;
using IntelliPM.Data.DTOs.AiResponseHistory.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiResponseHistoryServices
{
    public interface IAiResponseHistoryService
    {
        Task<List<AiResponseHistoryResponseDTO>> GetAllAsync();
        Task<AiResponseHistoryResponseDTO> GetByIdAsync(int id);
        Task<List<AiResponseHistoryResponseDTO>> GetByAiFeatureAsync(string aiFeature);
        Task<List<AiResponseHistoryResponseDTO>> GetByProjectIdAsync(int projectId);
        Task<List<AiResponseHistoryResponseDTO>> GetByCreatedByAsync(int createdBy);
        Task<AiResponseHistoryResponseDTO> CreateAsync(AiResponseHistoryRequestDTO request, string token);
        Task<AiResponseHistoryResponseDTO> UpdateAsync(int id, AiResponseHistoryRequestDTO request);
        Task DeleteAsync(int id);
    }
}
