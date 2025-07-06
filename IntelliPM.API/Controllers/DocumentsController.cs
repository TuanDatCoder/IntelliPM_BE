using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Services.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [Authorize]

    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }


        [HttpPost]
        public async Task<ActionResult<DocumentResponseDTO>> Create([FromBody] DocumentRequestDTO request)
        {
            var result = await _documentService.CreateDocument(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentResponseDTO>> Update(int id, [FromBody] UpdateDocumentRequest request)
        {
            var result = await _documentService.UpdateDocument(id, request);
            return Ok(result);
        }


        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetByProject(int projectId)
        {
            var result = await _documentService.GetDocumentsByProject(projectId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentResponseDTO>> GetById(int id)
        {
            var result = await _documentService.GetDocumentById(id);
            return Ok(result);
        }

        [HttpGet("created-by-me")]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetDocumentsCreatedByMe()
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var result = await _documentService.GetDocumentsCreatedByUser(userId);
            return Ok(result);

        }

        [HttpGet("{id}/summary")]
        public async Task<IActionResult> SummarizeDocument(int id)
        {
            try
            {
                var summary = await _documentService.SummarizeContent(id);
                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{documentId}/share")]
        public async Task<IActionResult> ShareDocument(int documentId, [FromBody] ShareDocumentRequestDTO req)
        {
            try
            {
                var result = await _documentService.ShareDocumentByEmail(documentId, req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{documentId}/submit-approval")]
        public async Task<IActionResult> SubmitForApproval(int documentId)
        {
            var userIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!int.TryParse(userIdClaim, out var approverId))
                return BadRequest("Invalid approver ID");

            var result = await _documentService.SubmitForApproval(documentId);
            return Ok(result);
        }


        [HttpPost("{documentId}/approve")]
        public async Task<IActionResult> ApproveOrReject(int documentId, [FromBody] UpdateDocumentStatusRequest req)
        {
            var result = await _documentService.UpdateApprovalStatus(documentId, req);
            return Ok(result);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetByStatus(string status)
        {
            var validStatuses = new[] { "Draft", "PendingApproval", "Approved", "Rejected" };
            if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Invalid status");

            var result = await _documentService.GetDocumentsByStatus(status);
            return Ok(result);
        }


        [HttpGet("project/{projectId}/status/{status}")]
        public async Task<IActionResult> GetByStatusAndProject(int projectId, string status)

        {
            var validStatuses = new[] { "Draft", "PendingApproval", "Approved", "Rejected" };
            if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Invalid status");

            var result = await _documentService.GetDocumentsByStatusAndProject(status, projectId);
            return Ok(result);
        }
    }
}
