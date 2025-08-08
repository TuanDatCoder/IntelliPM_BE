using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.DocumentComment;
using IntelliPM.Services.DocumentCommentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class DocumentCommentController : ControllerBase
    {
        private readonly IDocumentCommentService _service;

        public DocumentCommentController(IDocumentCommentService service)
        {
            _service = service;
        }

        [HttpGet("document/{documentId}")]
        public async Task<IActionResult> GetCommentsByDocument(int documentId)
        {
            var result = await _service.GetByDocumentIdAsync(documentId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] DocumentCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var created = await _service.CreateAsync(request, userId);
            return Ok(created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] DocumentCommentRequestDTO request)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var updated = await _service.UpdateAsync(id, request, userId);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var deleted = await _service.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound(new { message = "Not authorized or comment not found" });

            return Ok(new { message = "Deleted successfully" });
        }

    }

}
