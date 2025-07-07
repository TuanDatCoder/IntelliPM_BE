//using IntelliPM.Data.DTOs.MeetingTranscript.Request;
//using IntelliPM.Data.DTOs.MeetingTranscript.Response;
//using IntelliPM.Services.MeetingTranscriptServices;
//using Microsoft.AspNetCore.Mvc;

//namespace IntelliPM.API.Controllers
//{
//    [ApiController]
//    [Route("api/meeting-transcripts")]
//    public class MeetingTranscriptController : ControllerBase
//    {
//        private readonly IMeetingTranscriptService _service;

//        public MeetingTranscriptController(IMeetingTranscriptService service)
//        {
//            _service = service;
//        }

//        [HttpPost]
//        public async Task<IActionResult> UploadTranscript([FromForm] MeetingTranscriptRequestDTO request)
//        {
//            try
//            {
//                var result = await _service.UploadTranscriptAsync(request);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error in UploadTranscript: " + ex.Message);
//                return StatusCode(500, new { message = "An error occurred while uploading the transcript." });
//            }
//        }

//        [HttpGet("{meetingId}")]
//        public async Task<IActionResult> GetTranscriptByMeetingId(int meetingId)
//        {
//            try
//            {
//                var result = await _service.GetTranscriptByMeetingIdAsync(meetingId);
//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error in GetTranscriptByMeetingId: " + ex.Message);
//                return StatusCode(500, new { message = "An error occurred while retrieving the transcript." });
//            }
//        }
//    }
//}

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
    }
}
