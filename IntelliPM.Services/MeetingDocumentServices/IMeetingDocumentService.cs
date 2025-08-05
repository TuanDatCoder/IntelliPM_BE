using IntelliPM.Data.DTOs.MeetingDocument;

namespace IntelliPM.Services.MeetingDocumentServices
{
    public interface IMeetingDocumentService
    {
        Task<List<MeetingDocumentResponseDTO>> GetAllAsync();
        Task<MeetingDocumentResponseDTO?> GetByMeetingIdAsync(int meetingId);
        Task<MeetingDocumentResponseDTO> CreateAsync(MeetingDocumentRequestDTO request);
        Task<MeetingDocumentResponseDTO> UpdateAsync(int meetingId, MeetingDocumentRequestDTO request);
        Task DeleteAsync(int meetingId);
    }
}