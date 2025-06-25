using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
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
        Task<RiskResponseDTO> GetByIdAsync(int id);
        Task AddAsync(RiskRequestDTO request);
        Task UpdateAsync(int id, RiskRequestDTO request);
        Task DeleteAsync(int id);
    }
}
