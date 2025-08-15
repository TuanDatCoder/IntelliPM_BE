using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingSummaryServices
{
    public interface IMeetingSummaryService
    {
        Task<MeetingSummaryResponseDTO> CreateSummaryAsync(MeetingSummaryRequestDTO dto);
        Task<MeetingSummaryResponseDTO?> GetSummaryByTranscriptIdAsync(int meetingTranscriptId);
        Task<List<MeetingSummaryResponseDTO>?> GetSummariesByAccountIdAsync(int accountId);

        Task<List<MeetingSummaryResponseDTO>> GetAllMeetingSummariesByAccountIdAsync(int accountId);
        Task<MeetingSummaryResponseDTO?> GetMeetingSummaryByMeetingIdAsync(int meetingId);
        Task<bool> DeleteSummaryAndTranscriptAsync(int meetingTranscriptId);

    }
}