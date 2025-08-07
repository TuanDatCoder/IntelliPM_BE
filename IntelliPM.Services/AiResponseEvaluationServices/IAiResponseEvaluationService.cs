using IntelliPM.Data.DTOs.AiResponseEvaluation.Request;
using IntelliPM.Data.DTOs.AiResponseEvaluation.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiResponseEvaluationServices
{
    public interface IAiResponseEvaluationService
    {
        Task<List<AiResponseEvaluationResponseDTO>> GetAllAsync();
        Task<AiResponseEvaluationResponseDTO> GetByIdAsync(int id);
        Task<List<AiResponseEvaluationResponseDTO>> GetByAiResponseIdAsync(int aiResponseId);
        Task<List<AiResponseEvaluationResponseDTO>> GetByAccountIdAsync(int accountId);
        Task<AiResponseEvaluationResponseDTO> CreateAsync(AiResponseEvaluationRequestDTO request, string token);
        Task<AiResponseEvaluationResponseDTO> UpdateAsync(int id, AiResponseEvaluationRequestDTO request);
        Task DeleteAsync(int id);
    }
}
