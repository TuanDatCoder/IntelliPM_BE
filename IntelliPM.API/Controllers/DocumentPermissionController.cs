using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.DocumentPermission;
using IntelliPM.Services.DocumentPermissionServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/document-permissions")]
    public class DocumentPermissionsController : ControllerBase
    {
        private readonly IDocumentPermissionService _service;

        public DocumentPermissionsController(IDocumentPermissionService service)
        {
            _service = service;
        }

        [HttpPut("update-permission-type")]
        public async Task<IActionResult> UpdatePermissionType([FromBody] UpdatePermissionTypeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid request data.",
                    Data = ModelState
                });
            }

            try
            {
                var updated = await _service.UpdatePermissionTypeAsync(request.DocumentId, request.PermissionType);

                if (!updated)
                {
                    return NotFound(new ApiResponseDTO
                    {
                        IsSuccess = false,
                        Code = 404,
                        Message = "Permission not found for update."
                    });
                }

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Permission updated successfully.",
                    Data = new
                    {
                        request.DocumentId,
                        request.PermissionType
                    }
                });
            }
            catch (ArgumentException ax)
            {
                return UnprocessableEntity(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 422,
                    Message = ax.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = "Internal server error.",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("shared-users")]
        public async Task<IActionResult> GetSharedUsers([FromQuery] int documentId)
        {
            if (documentId <= 0)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 400,
                    message = "Invalid documentId"
                });
            }

            try
            {
                var users = await _service.GetSharedUsersAsync(documentId);

                if (users == null || !users.Any())
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        code = 404,
                        message = "No shared users found for this document"
                    });
                }

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    data = users,
                    message = "Retrieved shared users successfully"
                });
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return StatusCode(500, new
                {
                    isSuccess = false,
                    code = 500,
                    message = "An error occurred while fetching shared users",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("{id}/permission-type")]
        public async Task<IActionResult> GetPermissionType(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    code = 400,
                    message = "Invalid document ID."
                });
            }

            try
            {
                var permissionType = await _service.GetPermissionTypeAsync(id);

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    documentId = id,
                    permissionType
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    code = 404,
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    isSuccess = false,
                    code = 401,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccess = false,
                    code = 500,
                    message = "Internal server error: " + ex.Message
                });
            }
        }


    }
}
