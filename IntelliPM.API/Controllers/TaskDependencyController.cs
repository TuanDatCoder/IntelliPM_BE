using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.TaskDependency.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.TaskDependencyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskDependencyController : ControllerBase
    {
        private readonly ITaskDependencyService _service;

        public TaskDependencyController(ITaskDependencyService service)
        {
            _service = service;
        }

        [HttpGet("by-linked-from")]
        public async Task<IActionResult> GetByLinkedFrom([FromQuery] string linkedFrom)
        {
            try
            {
                var result = await _service.GetByLinkedFromAsync(linkedFrom);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task Dependency retrieved successfully",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving task dependency: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskDependencyRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    data = result,
                    message = "Task Dependency created successfully"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task dependency: {ex.Message}"
                });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> CreateMany([FromBody] TaskDependencyBatchRequestDTO batchDto)
        {
            if (!ModelState.IsValid || batchDto.Dependencies == null || !batchDto.Dependencies.Any())
                return BadRequest(new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Invalid input or empty dependency list."
                });

            try
            {
                var result = await _service.CreateManyAsync(batchDto.Dependencies);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    data = result,
                    message = "Task dependencies created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task dependencies: {ex.Message}"
                });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteConnection([FromQuery] string linkedFrom, [FromQuery] string linkedTo)
        {
            try
            {
                var success = await _service.DeleteConnectionAsync(linkedFrom, linkedTo);
                if (!success)
                    return NotFound(new { message = "Dependency not found." });

                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error deleting task dependencies: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteTaskDependencyAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"TaskDependency with id {id} not found." });
                }
                return Ok(new { message = "Deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error deleting task dependencies: {ex.Message}"
                });
            }
        }


    }
}
