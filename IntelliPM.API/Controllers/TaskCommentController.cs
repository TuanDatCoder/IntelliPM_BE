using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskCheckListServices;
using IntelliPM.Services.TaskCommentServices;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IntelliPM.Data.DTOs.TaskComment.Request;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class TaskCommentController : ControllerBase
    {
        private readonly ITaskCommentService _service;

        public TaskCommentController(ITaskCommentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllTaskComment();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all task comment successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var taskComment = await _service.GetTaskCommentById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task comment retrieved successfully",
                    Data = taskComment
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateTaskComment(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task comment created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task comment: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TaskCommentRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateTaskComment(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task comment updated successfully",
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
                    Message = $"Error updating task comment: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteTaskComment(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task comment deleted successfully"
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
                    Message = $"Error deleting task comment: {ex.Message}"
                });
            }
        }
    }
}
