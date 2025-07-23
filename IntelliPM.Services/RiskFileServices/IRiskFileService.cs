using IntelliPM.Data.DTOs.RiskFile.Request;
using IntelliPM.Data.DTOs.RiskFile.Response;
using IntelliPM.Data.DTOs.TaskFile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskFileServices
{
    public interface IRiskFileService
    {
        Task<RiskFileResponseDTO> UploadRiskFileAsync(RiskFileRequestDTO request);
        Task<bool> DeleteRiskFileAsync(int id);
        Task<List<RiskFileResponseDTO>> GetByRiskIdAsync(int riskId);
    }
}
