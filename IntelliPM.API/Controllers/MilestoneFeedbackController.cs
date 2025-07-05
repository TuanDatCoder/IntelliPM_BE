using IntelliPM.Data.DTOs.MilestoneFeedback.Request;
using IntelliPM.Services.MilestoneFeedbackServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MilestoneFeedbackController : ControllerBase
    {
        private readonly IMilestoneFeedbackService _service;

        public MilestoneFeedbackController(IMilestoneFeedbackService service)
        {
            _service = service;
        }

        [HttpPost("submit-feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] MilestoneFeedbackRequestDTO request)
        {
            var result = await _service.SubmitFeedbackAsync(request);
            return Ok(result);
        }

        [HttpPost("approve-milestone")]
        public async Task<IActionResult> ApproveMilestone(int meetingId, int accountId)
        {
            var result = await _service.ApproveMilestoneAsync(meetingId, accountId);
            return Ok(result);
        }

        [HttpGet("get-feedback/{meetingId}")]
        public async Task<IActionResult> GetFeedbackByMeetingId(int meetingId)
        {
            var feedback = await _service.GetFeedbackByMeetingIdAsync(meetingId);
            if (feedback == null)
                return NotFound(new { Message = $"No feedback found for Meeting ID {meetingId}" });

            return Ok(feedback);
        }
        [HttpPut("update-feedback/{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] MilestoneFeedbackRequestDTO request)
        {
            var result = await _service.UpdateFeedbackAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("delete-feedback/{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            await _service.DeleteFeedbackAsync(id);
            return NoContent();
        }

        [HttpGet("meeting/{meetingId}/rejected-feedbacks")]
        public async Task<IActionResult> GetRejectedFeedbacks(int meetingId)
        {
            var result = await _service.GetRejectedFeedbacksByMeetingIdAsync(meetingId);
            return Ok(result);
        }
    }
}