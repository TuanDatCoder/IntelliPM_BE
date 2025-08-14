using IntelliPM.Data.DTOs.MeetingSummary.Request;
using IntelliPM.Services.MeetingSummaryServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meeting-summaries")]
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

    }
}