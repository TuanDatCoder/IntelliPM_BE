using IntelliPM.Data.DTOs;
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
            if (documentId <= 0)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid documentId." });

            var data = await _service.GetByDocumentIdAsync(documentId);

            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Data = data ?? new List<DocumentCommentResponseDTO>(),
                Message = "OK"
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] DocumentCommentRequestDTO request)
        {
            // 1) Validate null & model state (trả về ApiResponseDTO)
            if (request == null)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Request body is required."
                });
            }

            if (!ModelState.IsValid)
            {
                // Nếu muốn trả chi tiết lỗi model:
                // var errors = ModelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Validation failed."
                });
            }

            // 2) Lấy userId từ JWT
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
            {
                return Unauthorized(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "Unauthorized."
                });
            }

            if (!int.TryParse(accountIdClaim, out var userId))
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid user ID format."
                });
            }

            // 3) Domain validations đồng nhất cách trả lỗi như GetCommentsByDocument
            if (request.DocumentId <= 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid documentId."
                });
            }

            // Nếu FromPos/ToPos là int? trong DTO, nhớ [Required] để tránh default 0
            if (request.FromPos < 0 || request.ToPos < 0)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "FromPos and ToPos must be >= 0."
                });
            }

            if (request.FromPos > request.ToPos)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "FromPos must be <= ToPos."
                });
            }

            try
            {
                var created = await _service.CreateAsync(request, userId);

                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Created",
                    Data = created
                });
            }
            catch (KeyNotFoundException kx)
            {
                return NotFound(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 404,
                    Message = kx.Message
                });
            }
            catch (ArgumentException ax)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = ax.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = ex.Message
                });
            }
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
        public async Task<IActionResult> UpdateComment(
    int id,
    [FromBody] UpdateDocumentCommentRequestDTO request)
        {
            if (request == null)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Request body is required." });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Validation failed." });

            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
                return Unauthorized(new ApiResponseDTO { IsSuccess = false, Code = 401, Message = "Unauthorized." });

            if (!int.TryParse(accountIdClaim, out var userId))
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid user ID format." });

            try
            {
                var updated = await _service.UpdateAsync(id, request, userId);
                return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Updated", Data = updated });
            }
            catch (KeyNotFoundException kx)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = kx.Message });
            }
            catch (UnauthorizedAccessException ux)
            {
                return StatusCode(403, new ApiResponseDTO { IsSuccess = false, Code = 403, Message = ux.Message });
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
        public async Task<IActionResult> DeleteComment(int id)
        {
            var accountIdClaim = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
            {
                return Unauthorized(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "Unauthorized."
                });
            }

            if (!int.TryParse(accountIdClaim, out var userId))
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid user ID format."
                });
            }

            try
            {
                var deleted = await _service.DeleteAsync(id, userId);
                if (!deleted)
                {
                    // Không rõ là 404 hay 403 -> trả 404 + message chung
                    return NotFound(new ApiResponseDTO
                    {
                        IsSuccess = false,
                        Code = 404,
                        Message = "Not authorized or comment not found."
                    });
                }

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Deleted successfully",
                    Data = new { id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = ex.Message
                });
            }
        }


    }

}
