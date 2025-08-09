using IntelliPM.Data.DTOs.EpicComment.Request;
using IntelliPM.Data.DTOs.EpicComment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicCommentServices
{
    public interface IEpicCommentService
    {
        Task<EpicCommentResponseDTO> CreateEpicComment(EpicCommentRequestDTO request);
        Task DeleteEpicComment(int id,int createdBy);
        Task<List<EpicCommentResponseDTO>> GetAllEpicComment(int page = 1, int pageSize = 10);
        Task<EpicCommentResponseDTO> GetEpicCommentById(int id);
        Task<EpicCommentResponseDTO> UpdateEpicComment(int id, EpicCommentRequestDTO request);
        Task<List<EpicCommentResponseDTO>> GetEpicCommentByEpicIdAsync(string epicId);
    }
}
