using IntelliPM.Data.DTOs;
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



        //[HttpPost("request")]
        //public async Task<ActionResult<DocumentResponseDTO>> CreateDocumentRequest([FromBody] DocumentRequestDTO request)
        //{
        //    var accountIdClaim = User.FindFirst("accountId")?.Value;

        //    if (string.IsNullOrEmpty(accountIdClaim))
        //        return Unauthorized();

        //    if (!int.TryParse(accountIdClaim, out var userId))
        //        return BadRequest("Invalid user ID format");

        //    var result = await _documentService.CreateDocumentRequest(request, userId); 
        //    return Ok(result);
        //}


        [HttpPost("create")]
        public async Task<IActionResult> CreateDocument([FromBody] DocumentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid data", Data = ModelState });

            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized("Missing accountId in token.");
            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest("Invalid user ID format.");

            try
            {
                var dto = await _documentService.CreateDocument(request, userId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Created",
                    Data = dto
                });
            }
            catch (ArgumentException ax)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ax.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = ex.Message });
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid data", Data = ModelState });

            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim)) return Unauthorized("Missing accountId in token.");
            if (!int.TryParse(accountIdClaim, out var userId)) return BadRequest("Invalid user ID format.");

            try
            {
                var dto = await _documentService.UpdateDocument(id, request, userId);
                return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Updated", Data = dto });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ax)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ax.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                });

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid user ID"
                });

            try
            {
                await _documentService.DeleteDocument(id, userId);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = StatusCodes.Status200OK,
                    Message = "Document deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }




        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetByProject(int projectId)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;

            if (string.IsNullOrEmpty(accountIdClaim))
                return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized" });

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid user ID format" });

            var result = await _documentService.GetDocumentsByProject(projectId, userId);

            var data = result ?? new List<DocumentResponseDTO>();

            return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Success", Data = result });
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // var accountIdClaim = User.FindFirst("accountId")?.Value;
                // if (string.IsNullOrEmpty(accountIdClaim))
                //     return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized" });
                // var userId = int.Parse(accountIdClaim);

                var doc = await _documentService.GetDocumentById(id);

                if (doc == null)
                {
                    return NotFound(new ApiResponseDTO
                    {
                        IsSuccess = false,
                        Code = StatusCodes.Status404NotFound,
                        Message = $"Document {id} not found."
                    });
                }

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = StatusCodes.Status200OK,
                    Message = "Success",
                    Data = doc
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status403Forbidden,
                    Message = "Forbidden."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }


        //[HttpGet("created-by-me")]
        //public async Task<ActionResult<List<DocumentResponseDTO>>> GetDocumentsCreatedByMe()
        //{
        //    var accountIdClaim = User.FindFirst("accountId")?.Value;

        //    if (string.IsNullOrEmpty(accountIdClaim))
        //        return Unauthorized();

        //    if (!int.TryParse(accountIdClaim, out var userId))
        //        return BadRequest("Invalid user ID format");

        //    var result = await _documentService.GetDocumentsCreatedByUser(userId);
        //    return Ok(result);

        //}

        //[HttpGet("{id}/summary")]
        //public async Task<IActionResult> SummarizeDocument(int id)
        //{
        //    try
        //    {
        //        var summary = await _documentService.SummarizeContent(id);
        //        return Ok(new { summary });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        [HttpPost("{documentId}/share")]
        public async Task<IActionResult> ShareDocument(int documentId, [FromBody] ShareDocumentRequestDTO req)
        {
            try
            {
                var result = await _documentService.ShareDocumentByEmail(documentId, req);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = result.Success,
                    Code = StatusCodes.Status200OK,
                    Message = result.Success
                        ? "Share document emails sent successfully."
                        : "Share document emails sent with some failures.",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status404NotFound,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Message = ex.Message
                });
            }
        }


        //[Authorize]
        //[HttpPost("{documentId}/submit-approval")]
        //public async Task<IActionResult> SubmitForApproval(int documentId)
        //{
        //    var userIdClaim = User.FindFirst("accountId")?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim))
        //        return Unauthorized();

        //    if (!int.TryParse(userIdClaim, out var approverId))
        //        return BadRequest("Invalid approver ID");

        //    var result = await _documentService.SubmitForApproval(documentId);
        //    return Ok(result);
        //}


        //[HttpPost("{documentId}/approve")]
        //public async Task<IActionResult> ApproveOrReject(int documentId, [FromBody] UpdateDocumentStatusRequest req)
        //{
        //    var result = await _documentService.UpdateApprovalStatus(documentId, req);
        //    return Ok(result);
        //}


        //[HttpGet("status/{status}")]
        //public async Task<ActionResult<List<DocumentResponseDTO>>> GetByStatus(string status)
        //{
        //    var validStatuses = new[] { "Draft", "PendingApproval", "Approved", "Rejected" };
        //    if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        //        return BadRequest("Invalid status");

        //    var result = await _documentService.GetDocumentsByStatus(status);
        //    return Ok(result);
        //}


        //[HttpGet("project/{projectId}/status/{status}")]
        //public async Task<IActionResult> GetByStatusAndProject(int projectId, string status)

        //{
        //    var validStatuses = new[] { "Draft", "PendingApproval", "Approved", "Rejected" };
        //    if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        //        return BadRequest("Invalid status");


        //    var result = await _documentService.GetDocumentsByStatusAndProject(status, projectId);
        //    return Ok(result);
        //}

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

        //[HttpGet("find-by-key")]
        //public async Task<ActionResult<DocumentResponseDTO>> GetByKey(
        //    [FromQuery] int projectId,
        //    [FromQuery] string? epicId,
        //    [FromQuery] string? taskId,
        //    [FromQuery] string? subTaskId)      
        //{
        //    var result = await _documentService.GetByKey(projectId, epicId, taskId, subTaskId);
        //    if (result == null)
        //        return NotFound("Document not found");

        //    return Ok(result);
        //}

        [HttpGet("mapping")]
        public async Task<IActionResult> GetDocumentMapping([FromQuery] int projectId, [FromQuery] int userId)
        {
            var mapping = await _documentService.GetUserDocumentMappingAsync(projectId, userId);
            return Ok(mapping);
        }


        //[HttpGet("status-total")]
        //public async Task<ActionResult<Dictionary<string, int>>> GetStatusSummary()
        //{
        //    var summary = await _documentService.GetStatusCount();
        //    return Ok(summary);
        //}

        //[HttpGet("project/{projectId}/status-total")]
        //public async Task<ActionResult<Dictionary<string, int>>> GetStatusSummaryByProject(int projectId)
        //{
        //    var result = await _documentService.GetStatusCountByProject(projectId);
        //    return Ok(result);
        //}

        [HttpPost("{id}/generate-from-project")]
        public async Task<ActionResult<GenerateDocumentResponse>> GenerateFromProject(int id)
        {
            var result = await _documentService.GenerateFromProject(id);
            return Ok(result); 
        }

        [HttpPost("{id}/generate-from-task")]
        public async Task<ActionResult<GenerateDocumentResponse>> GenerateFromTask(int id)
        {
            var result = await _documentService.GenerateFromTask(id);
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

        [HttpPatch("{id:int}/visibility")]
        public async Task<IActionResult> ChangeVisibility([FromRoute] int id, [FromBody] ChangeVisibilityRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid payload." });

            var userIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            if (!int.TryParse(userIdClaim, out var userId)) return BadRequest("Invalid account ID");

            try
            {
                var dto = await _documentService.ChangeVisibilityAsync(id, request, userId);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Visibility changed successfully.",
                    Data = dto
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(); // 403
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                // TODO: log ex.ToString()
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = ex.Message  // t?m th?i ?? debug
                });
            }

        }


        [HttpGet("shared-to-me")]
        public async Task<IActionResult> GetDocumentsSharedToMe()
        {
            var userIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized: missing accountId claim",
                    Data = null
                });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid account ID",
                    Data = null
                });
            }

            try
            {
                var docs = await _documentService.GetDocumentsSharedToUser(userId);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = StatusCodes.Status200OK,
                    Message = docs.Count > 0
                        ? "Documents shared to you retrieved successfully."
                        : "No documents shared to you.",
                    Data = docs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpGet("shared-to-me/project/{projectId}")]
        public async Task<IActionResult> GetDocumentsSharedToMeInProject(int projectId)
        {
            var userIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized: missing accountId claim",
                    Data = null
                });
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Invalid account ID",
                    Data = null
                });
            }

            try
            {
                var docs = await _documentService.GetDocumentsSharedToUserInProject(userId, projectId);

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = StatusCodes.Status200OK,
                    Message = docs.Count > 0
                        ? "Documents shared to you in this project retrieved successfully."
                        : "No documents shared to you in this project.",
                    Data = docs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                });
            }
        }








    }
}
