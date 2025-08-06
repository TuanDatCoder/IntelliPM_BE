using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.EpicComment.Request;
using IntelliPM.Services.EpicCommentServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpicCommentController : ControllerBase
    {
        private readonly IEpicCommentService _service;

        public EpicCommentController(IEpicCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid page or page size" });
            var result = await _service.GetAllEpicComment(page, pageSize);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all epic comment successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var epicComment = await _service.GetEpicCommentById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Epic comment retrieved successfully",
                    Data = epicComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EpicCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateEpicComment(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Epic comment created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating epic comment: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EpicCommentRequestDTO request)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var updated = await _service.UpdateEpicComment(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic comment updated successfully",
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
                    Message = $"Error updating epic comment: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                await _service.DeleteEpicComment(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic comment deleted successfully"
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
                    Message = $"Error deleting epic comment: {ex.Message}"
                });
            }
        }
        [HttpGet("by-epic/{epicId}")]
        public async Task<IActionResult> GetSubtaskCommentBySubtaskId(string epicId)
        {
            try
            {
                var epicComments = await _service.GetEpicCommentByEpicIdAsync(epicId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved epic comment successfully.",
                    Data = epicComments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving epic comment: {ex.Message}"
                });
            }
        }
    }
}
