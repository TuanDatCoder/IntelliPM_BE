using IntelliPM.Data.DTOs.RiskComment.Request;
using IntelliPM.Data.DTOs.RiskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskCommentServices
{
    public interface IRiskCommentService
    {
        Task<List<RiskCommentResponseDTO>> GetAllRiskComment();
        Task<RiskCommentResponseDTO> GetById(int id);
        Task<RiskCommentResponseDTO> CreateRiskComment(RiskCommentRequestDTO request);
        Task<RiskCommentResponseDTO> UpdateRiskComment(int id, RiskCommentRequestDTO request);
        Task DeleteRiskComment(int id);
        Task<List<RiskCommentResponseDTO>> GetByRiskIdAsync(int riskId);
    }
}
