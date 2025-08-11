using IntelliPM.Data.DTOs.DocumentComment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentCommentServices
{
    public interface IDocumentCommentService
    {
        Task<List<DocumentCommentResponseDTO>> GetByDocumentIdAsync(int documentId);

        Task<DocumentCommentResponseDTO?> GetByIdAsync(int id);
        Task<DocumentCommentResponseDTO> UpdateAsync(
          int id,
          UpdateDocumentCommentRequestDTO request,
          int userId);

        Task<bool> DeleteAsync(int id, int userId);


        Task<DocumentCommentResponseDTO> CreateAsync(DocumentCommentRequestDTO request, int userId);
    }
}
