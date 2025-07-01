using IntelliPM.Data.DTOs.SubtaskComment.Request;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskCommentServices
{
    public interface ISubtaskCommentService
    {
        Task<List<SubtaskCommentResponseDTO>> GetAllSubtaskComment();
        Task<SubtaskCommentResponseDTO> GetSubtaskCommentById(int id);
        Task<SubtaskCommentResponseDTO> CreateSubtaskComment(SubtaskCommentRequestDTO request);
        Task<SubtaskCommentResponseDTO> UpdateSubtaskComment(int id, SubtaskCommentRequestDTO request);
        Task DeleteSubtaskComment(int id);
        Task<List<SubtaskCommentResponseDTO>> GetSubtaskCommentBySubtaskIdAsync(string subtaskId);
    }
}
