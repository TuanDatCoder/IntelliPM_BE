using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Data.DTOs.MilestoneFeedback.Response;

namespace IntelliPM.Services.MilestoneFeedbackServices
{
    public interface IMilestoneFeedbackService
    {
        Task<MilestoneFeedbackResponseDTO> SubmitFeedbackAsync(MilestoneFeedbackRequestDTO request);
        Task<MilestoneFeedbackResponseDTO> ApproveMilestoneAsync(int meetingId, int accountId);
    }
}