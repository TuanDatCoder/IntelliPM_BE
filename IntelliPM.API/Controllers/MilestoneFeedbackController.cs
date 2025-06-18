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
    }
}