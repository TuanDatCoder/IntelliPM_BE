using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Services.MeetingSummaryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meeting-summaries")]
    [Authorize]
    public class MeetingSummaryController : ControllerBase
    {
        private readonly IMeetingSummaryService _service;

        public MeetingSummaryController(IMeetingSummaryService service)
        {
            _service = service;
        }

        [HttpGet("{meetingTranscriptId}")]
        public async Task<IActionResult> GetByTranscriptId(int meetingTranscriptId)
        {
            var result = await _service.GetSummaryByTranscriptIdAsync(meetingTranscriptId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            var result = await _service.GetSummariesByAccountIdAsync(accountId);
            return Ok(result);
        }

        [HttpGet("all-by-account/{accountId}")]
        public async Task<IActionResult> GetAllByAccount(int accountId)
        {
            var result = await _service.GetAllMeetingSummariesByAccountIdAsync(accountId);
            return Ok(result);
        }

        [HttpDelete("{meetingTranscriptId}")]
        public async Task<IActionResult> DeleteByTranscriptId(int meetingTranscriptId)
        {
            var isDeleted = await _service.DeleteSummaryAndTranscriptAsync(meetingTranscriptId);
            if (!isDeleted)
                return NotFound();
            return NoContent();
        }
        [HttpGet("detail/{meetingId}")]
        public async Task<IActionResult> GetDetail(int meetingId)
        {
            var result = await _service.GetMeetingSummaryByMeetingIdAsync(meetingId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        // PUT /api/meeting-summaries/{meetingTranscriptId}
        [HttpPut("{meetingTranscriptId}")]
        public async Task<IActionResult> UpdateSummary(int meetingTranscriptId, [FromBody] UpdateMeetingSummaryRequestDTO request)
        {
            try
            {
                request.MeetingTranscriptId = meetingTranscriptId;
                return Ok(await _service.UpdateSummaryAsync(request));
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý lỗi optimistic concurrency
                return Conflict(new { message = ex.Message, error = ex.Message });
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error updating summary.", error = ex.Message });
            }
        }

        // GET /api/meeting-summaries/{meetingTranscriptId}/history
        [HttpGet("{meetingTranscriptId}/history")]
        public async Task<IActionResult> GetSummaryHistory(int meetingTranscriptId)
        {
            try
            {
                return Ok(await _service.GetSummaryHistoryAsync(meetingTranscriptId));
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error retrieving history.", error = ex.Message });
            }
        }

        // POST /api/meeting-summaries/{meetingTranscriptId}/restore
        [HttpPost("{meetingTranscriptId}/restore")]
        public async Task<IActionResult> RestoreSummary(int meetingTranscriptId, [FromBody] RestoreSummaryRequestDTO request)
        {
            try
            {
                request.MeetingTranscriptId = meetingTranscriptId;
                return Ok(await _service.RestoreSummaryAsync(request));
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "Error restoring summary.", error = ex.Message });
            }
        }

    }
}