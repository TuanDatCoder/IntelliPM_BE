using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.MilestoneComment.Request;
using IntelliPM.Services.MilestoneCommentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MilestoneCommentController : ControllerBase
    {
        private readonly IMilestoneCommentService _service;

        public MilestoneCommentController(IMilestoneCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid page or page size" });
            var result = await _service.GetAllMilestoneComment(page, pageSize);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all milestone comments successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var milestoneComment = await _service.GetMilestoneCommentById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Milestone comment retrieved successfully",
                    Data = milestoneComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MilestoneCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateMilestoneComment(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Milestone comment created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating milestone comment: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MilestoneCommentRequestDTO request)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var updated = await _service.UpdateMilestoneComment(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Milestone comment updated successfully",
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
                    Message = $"Error updating milestone comment: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                await _service.DeleteMilestoneComment(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Milestone comment deleted successfully"
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
                    Message = $"Error deleting milestone comment: {ex.Message}"
                });
            }
        }

        [HttpGet("by-milestone/{milestoneId}")]
        public async Task<IActionResult> GetMilestoneCommentByMilestoneId(int milestoneId)
        {
            try
            {
                var milestoneComments = await _service.GetMilestoneCommentByMilestoneIdAsync(milestoneId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved milestone comments successfully.",
                    Data = milestoneComments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving milestone comments: {ex.Message}"
                });
            }
        }
    }
}
