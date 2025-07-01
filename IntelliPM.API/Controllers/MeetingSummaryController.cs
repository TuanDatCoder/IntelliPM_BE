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

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] MeetingSummaryRequestDTO request)
        //{
        //    var result = await _service.CreateSummaryAsync(request);
        //    return Ok(result);
        //}

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
    }
}