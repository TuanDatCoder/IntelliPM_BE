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
        Task<List<RiskSolutionResponseDTO>> GetListByRiskIdAsync(int riskId);
        Task<RiskSolutionResponseDTO> CreateAsync(RiskSolutionRequestDTO request);
        Task<RiskSolutionResponseDTO> UpdateAsync(int id, RiskSolutionRequestDTO request);
    }
}
