using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _service;

        public TaskController(ITaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllTasks();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all tasks successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var task = await _service.GetTaskById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task retrieved successfully",
                    Data = task
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-title")]
        public async Task<IActionResult> GetByTitle([FromQuery] string title)
        {
            try
            {
                var tasks = await _service.GetTaskByTitle(title);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Tasks retrieved successfully",
                    Data = tasks
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
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateTask(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating task: {ex.Message}"
                });
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] List<TaskRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Request list cannot be null or empty." });
            }

            try
            {
                var results = new List<TaskResponseDTO>();
                foreach (var request in requests)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
                    }
                    var result = await _service.CreateTask(request);
                    results.Add(result);
                }
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Tasks created successfully",
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating tasks: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TaskRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateTask(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task updated successfully",
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
                    Message = $"Error updating task: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteTask(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task deleted successfully"
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
                    Message = $"Error deleting task: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] string status)
        {
            try
            {
                var updated = await _service.ChangeTaskStatus(id, status);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task status updated successfully",
                    Data = updated
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
                    Message = $"Error updating task status: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/type")]
        public async Task<IActionResult> ChangeType(string id, [FromBody] string type)
        {
            try
            {
                var updated = await _service.ChangeTaskStatus(id, type);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task type updated successfully",
                    Data = updated
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
                    Message = $"Error updating task type: {ex.Message}"
                });
            }
        }

        [HttpGet("by-project-id")]
        public async Task<IActionResult> GetByProjectId(int projectId)
        {
            try
            {
                var tasks = await _service.GetTasksByProjectIdAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = $"Tasks for Project ID {projectId} retrieved successfully",
                    Data = tasks
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
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

    }
}
