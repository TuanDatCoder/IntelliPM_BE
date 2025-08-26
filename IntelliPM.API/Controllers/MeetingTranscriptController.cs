using IntelliPM.Data.DTOs.MeetingTranscript.Request;
using IntelliPM.Data.DTOs.MeetingTranscript.Response;
using IntelliPM.Services.MeetingTranscriptServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meeting-transcripts")]
    public class MeetingTranscriptController : ControllerBase
    {
        private readonly IMeetingTranscriptService _service;
        private readonly ILogger<MeetingTranscriptController> _logger;

        public MeetingTranscriptController(
            IMeetingTranscriptService service,
            ILogger<MeetingTranscriptController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UploadTranscript([FromForm] MeetingTranscriptRequestDTO request)
        {
            try
            {
                var result = await _service.UploadTranscriptAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadTranscript: {Message}", ex.Message);

                return StatusCode(500, new
                {
                    message = "An error occurred while uploading the transcript.",
                    error = ex.Message,
                    stack = ex.StackTrace
                });
            }
        }

        [HttpPost("from-url")]
        public async Task<IActionResult> UploadTranscriptFromUrl([FromBody] MeetingTranscriptFromUrlRequestDTO request)
        {
            try
            {
                var result = await _service.UploadTranscriptFromUrlAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadTranscriptFromUrl: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    message = "An error occurred while uploading transcript from video URL.",
                    error = ex.Message,
                    stack = ex.StackTrace
                });
            }
        }



        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetTranscriptByMeetingId(int meetingId)
        {
            try
            {
                var result = await _service.GetTranscriptByMeetingIdAsync(meetingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTranscriptByMeetingId: {Message}", ex.Message);

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the transcript.",
                    error = ex.Message
                });
            }
        }

        // PUT /api/meeting-transcripts/{meetingId}
        [HttpPut("{meetingId}")]
        public async Task<IActionResult> UpdateTranscript(int meetingId, [FromBody] UpdateMeetingTranscriptRequestDTO request)
        {
            try { request.MeetingId = meetingId; return Ok(await _service.UpdateTranscriptAsync(request)); }
            catch (Exception ex) { _logger.LogError(ex, "Error in UpdateTranscript"); return StatusCode(500, new { message = "Error updating transcript.", error = ex.Message }); }
        }

        // GET /api/meeting-transcripts/{meetingId}/history
        [HttpGet("{meetingId}/history")]
        public async Task<IActionResult> GetTranscriptHistory(int meetingId)
        {
            try { return Ok(await _service.GetTranscriptHistoryAsync(meetingId)); }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetTranscriptHistory"); return StatusCode(500, new { message = "Error retrieving history.", error = ex.Message }); }
        }

        // POST /api/meeting-transcripts/{meetingId}/restore
        [HttpPost("{meetingId}/restore")]
        public async Task<IActionResult> RestoreTranscript(int meetingId, [FromBody] RestoreTranscriptRequestDTO request)
        {
            try { request.MeetingId = meetingId; return Ok(await _service.RestoreTranscriptAsync(request)); }
            catch (Exception ex) { _logger.LogError(ex, "Error in RestoreTranscript"); return StatusCode(500, new { message = "Error restoring transcript.", error = ex.Message }); }
        }

    }
}