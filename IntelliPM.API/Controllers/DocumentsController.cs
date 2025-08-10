using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocumentViaEmail;
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

        [HttpGet]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetAll()
        {
            var result = await _documentService.GetAllDocuments();
            return Ok(result);
        }



        [HttpPost("request")]
        public async Task<ActionResult<DocumentResponseDTO>> CreateDocumentRequest([FromBody] DocumentRequestDTO request)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var result = await _documentService.CreateDocumentRequest(request, userId); 
            return Ok(result);
        }


        [HttpPost("create")]
        public async Task<ActionResult<DocumentResponseDTO>> CreateDocument([FromBody] DocumentRequestDTO request)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var result = await _documentService.CreateDocument(request, userId);
            return Ok(result);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest request)
        {
            var userId = int.Parse(User.FindFirst("accountId")?.Value ?? "0");
            var result = await _documentService.UpdateDocument(id, request, userId);
            return Ok(result);
        }



        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<DocumentResponseDTO>>> GetByProject(int projectId)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized();

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format");

            var result = await _documentService.GetDocumentsByProject(projectId, userId);
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

        [HttpPost("{id}/generate-ai-content")]
        public async Task<IActionResult> GenerateAIContent(int id, [FromBody] string prompt)
        {
            try
            {
                var content = await _documentService.GenerateAIContent(id, prompt);
                return Ok(new { content });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("ask-ai")]
        public async Task<IActionResult> AskAI([FromBody] string prompt)
        {
            try
            {
                var result = await _documentService.GenerateFreeAIContent(prompt);
                return Ok(new { content = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("find-by-key")]
        public async Task<ActionResult<DocumentResponseDTO>> GetByKey(
            [FromQuery] int projectId,
            [FromQuery] string? epicId,
            [FromQuery] string? taskId,
            [FromQuery] string? subTaskId)      
        {
            var result = await _documentService.GetByKey(projectId, epicId, taskId, subTaskId);
            if (result == null)
                return NotFound("Document not found");

            return Ok(result);
        }

        [HttpGet("mapping")]
        public async Task<IActionResult> GetDocumentMapping([FromQuery] int projectId, [FromQuery] int userId)
        {
            var mapping = await _documentService.GetUserDocumentMappingAsync(projectId, userId);
            return Ok(mapping);
        }


        [HttpGet("status-total")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStatusSummary()
        {
            var summary = await _documentService.GetStatusCount();
            return Ok(summary);
        }

        [HttpGet("project/{projectId}/status-total")]
        public async Task<ActionResult<Dictionary<string, int>>> GetStatusSummaryByProject(int projectId)
        {
            var result = await _documentService.GetStatusCountByProject(projectId);
            return Ok(result);
        }

        [HttpPost("{id}/generate-from-tasks")]
        public async Task<ActionResult<GenerateDocumentResponse>> GenerateFromTasks(int id)
        {
            var result = await _documentService.GenerateFromExistingDocument(id);
            return Ok(result); 
        }


        [HttpPost("share-via-email")]
        //public async Task<IActionResult> ShareDocumentViaEmail([FromBody] ShareDocumentViaEmailRequest req)
        //{
        //    await _documentService.ShareDocumentViaEmail(req);
        //    return Ok(new { message = "Emails sent successfully" });
        //}
        public async Task<IActionResult> ShareViaEmailWithFile([FromForm] ShareDocumentViaEmailRequest request)
        {
            try
            {
                await _documentService.ShareDocumentViaEmailWithFile(request);
                return Ok(new { message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{documentId}/permission/current-user")]
        public async Task<IActionResult> GetMyPermission(int documentId)
        {
            var userIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            if (!int.TryParse(userIdClaim, out var userId)) return BadRequest("Invalid account ID");

            var permission = await _documentService.GetUserPermissionLevel(documentId, userId);
            return Ok(new { permission = permission ?? "none" });
        }





    }
}
