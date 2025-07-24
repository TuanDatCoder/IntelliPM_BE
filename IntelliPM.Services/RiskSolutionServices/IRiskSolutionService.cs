using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskSolutionServices
{
    public interface IRiskSolutionService
    {
        Task<List<RiskSolutionResponseDTO>> GetByRiskIdAsync(int riskId);
        Task<RiskSolutionResponseDTO> CreateAsync(RiskSolutionRequestDTO dto);
        Task<RiskSolutionResponseDTO?> UpdateContigencyPlanAsync(int id, string contigencyPlan);
        Task<RiskSolutionResponseDTO?> UpdateMitigationPlanAsync(int id, string mitigationPlan);
        Task DeleteRiskSolution(int id);
    }
}
