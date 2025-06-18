using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Data.DTOs.MilestoneFeedback.Response;

namespace IntelliPM.Services.MilestoneFeedbackServices
{
    public interface IMilestoneFeedbackService
    {
        Task<MilestoneFeedbackResponseDTO> SubmitFeedbackAsync(MilestoneFeedbackRequestDTO request);
        Task<MilestoneFeedbackResponseDTO> ApproveMilestoneAsync(int meetingId, int accountId);
        Task<MilestoneFeedbackResponseDTO?> GetFeedbackByMeetingIdAsync(int meetingId);

        Task<MilestoneFeedbackResponseDTO> UpdateFeedbackAsync(int id, MilestoneFeedbackRequestDTO request);
        Task DeleteFeedbackAsync(int id);
    }
}