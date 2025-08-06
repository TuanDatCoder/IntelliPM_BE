using IntelliPM.Data.DTOs.MeetingDocument;
using IntelliPM.Services.MeetingDocumentServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meeting-documents")]
    public class MeetingDocumentController : ControllerBase
    {
        private readonly IMeetingDocumentService _service;

        public MeetingDocumentController(IMeetingDocumentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetByMeetingId(int meetingId)
        {
            var result = await _service.GetByMeetingIdAsync(meetingId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeetingDocumentRequestDTO request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }

        [HttpPut("{meetingId}")]
        public async Task<IActionResult> Update(int meetingId, [FromBody] MeetingDocumentRequestDTO request)
        {
            var result = await _service.UpdateAsync(meetingId, request);
            return Ok(result);
        }

        [HttpDelete("{meetingId}")]
        public async Task<IActionResult> Delete(int meetingId)
        {
            await _service.DeleteAsync(meetingId);
            return NoContent();
        }
    }
}