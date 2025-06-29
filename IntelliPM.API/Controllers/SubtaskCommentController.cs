using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskCommentServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IntelliPM.Services.SubtaskCommentServices;
using IntelliPM.Data.DTOs.SubtaskComment.Request;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SubtaskCommentController : ControllerBase
    {
        private readonly ISubtaskCommentService _service;

        public SubtaskCommentController(ISubtaskCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllSubtaskComment();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all subtask comment successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var subtaskComment = await _service.GetSubtaskCommentById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Subtask comment retrieved successfully",
                    Data = subtaskComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubtaskCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateSubtaskComment(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Subtask comment created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating subtask comment: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubtaskCommentRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateSubtaskComment(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask comment updated successfully",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error updating subtask comment: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteSubtaskComment(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask comment deleted successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error deleting subtask comment: {ex.Message}"
                });
            }
        }

        [HttpGet("by-subtask/{subtaskId}")]
        public async Task<IActionResult> GetSubtaskCommentBySubtaskId(string subtaskId)
        {
            try
            {
                var taskComments = await _service.GetSubtaskCommentBySubtaskIdAsync(subtaskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved task comment successfully.",
                    Data = taskComments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving task comment: {ex.Message}"
                });
            }
        }
    }
}
