using IntelliPM.Data.DTOs.MilestoneComment.Request;
using IntelliPM.Data.DTOs.MilestoneComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.MilestoneCommentServices
{
    public interface IMilestoneCommentService
    {
        Task<MilestoneCommentResponseDTO> CreateMilestoneComment(MilestoneCommentRequestDTO request);
        Task DeleteMilestoneComment(int id);
        Task<List<MilestoneCommentResponseDTO>> GetAllMilestoneComment(int page = 1, int pageSize = 10);
        Task<MilestoneCommentResponseDTO> GetMilestoneCommentById(int id);
        Task<List<MilestoneCommentResponseDTO>> GetMilestoneCommentByMilestoneIdAsync(int milestoneId);
        Task<MilestoneCommentResponseDTO> UpdateMilestoneComment(int id, MilestoneCommentRequestDTO request);
    }
}
