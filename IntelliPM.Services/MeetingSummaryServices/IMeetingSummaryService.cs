using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Data.DTOs.MeetingSummary.Response;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingSummaryServices
{
    public interface IMeetingSummaryService
    {
        Task<MeetingSummaryResponseDTO> CreateSummaryAsync(MeetingSummaryRequestDTO dto);
        Task<MeetingSummaryResponseDTO?> GetSummaryByTranscriptIdAsync(int meetingTranscriptId);
    }
}