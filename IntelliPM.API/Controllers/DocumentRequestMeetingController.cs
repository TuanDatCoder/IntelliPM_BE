using IntelliPM.Data.DTOs.DocumentRequestMeeting;
using IntelliPM.Data.Entities;
using IntelliPM.Services.DocumentRequestMeetingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/document-request-meetings")]
    public class DocumentRequestMeetingController : ControllerBase
    {
        private readonly IDocumentRequestMeetingService _service;

        public DocumentRequestMeetingController(IDocumentRequestMeetingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateDocumentRequestMeetingDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequestMeetingDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("pm/incoming")]
        public async Task<IActionResult> GetIncomingForPM(
      [FromQuery] string? status,
      [FromQuery] bool? sentToClient,
      [FromQuery] bool? clientViewed,
      [FromQuery] int? page,
      [FromQuery] int? pageSize)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim)) return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var pmId))
                return BadRequest("Invalid user ID in token.");

            var result = await _service.GetInboxForPMAsync(pmId, status, sentToClient, clientViewed, page, pageSize);
            return Ok(result);
        }
    }

}
