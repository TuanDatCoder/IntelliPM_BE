using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskServices
{
    public interface IRiskService
    {
        Task<List<RiskResponseDTO>> GetAllRisksAsync();
        Task<List<RiskResponseDTO>> GetByProjectIdAsync(int projectId);
        Task<List<RiskResponseDTO>> GetByProjectKeyAsync(string projectKey);
        Task<RiskResponseDTO> GetByIdAsync(int id);
        Task AddAsync(RiskRequestDTO request);
        Task UpdateAsync(int id, RiskRequestDTO request);
        Task DeleteAsync(int id);
        Task<List<RiskResponseDTO>> GetUnapprovedAIRisksAsync(int projectId);
        Task ApproveRiskAsync(RiskRequestDTO dto, RiskSolutionRequestDTO solutionDto);
        Task<List<RiskRequestDTO>> DetectAndSaveProjectRisksAsync(int projectId);
        Task<List<RiskRequestDTO>> DetectProjectRisksAsync(int projectId);
        Task<List<RiskRequestDTO>> SaveProjectRisksAsync(List<RiskRequestDTO> risks);
    }
}
