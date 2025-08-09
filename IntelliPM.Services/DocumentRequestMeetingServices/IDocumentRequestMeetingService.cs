using IntelliPM.Data.DTOs.DocumentRequestMeeting;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentRequestMeetingServices
{
    public interface IDocumentRequestMeetingService
    {
        Task<DocumentRequestMeetingResponseDTO> CreateAsync(CreateDocumentRequestMeetingDTO dto);
        Task<DocumentRequestMeetingResponseDTO> UpdateAsync(int id, UpdateDocumentRequestMeetingDTO dto);
        Task<List<DocumentRequestMeetingResponseDTO>> GetAllAsync();
        Task<DocumentRequestMeetingResponseDTO?> GetByIdAsync(int id);

        Task<List<DocumentRequestMeetingResponseDTO>> GetInboxForPMAsync(
    int pmId, string? status = null, bool? sentToClient = null, bool? clientViewed = null,
    int? page = null, int? pageSize = null);
        Task DeleteAsync(int id);
    }
}
