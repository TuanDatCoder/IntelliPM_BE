using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Services.TaskAssignmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/task/{taskId}/[controller]")]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly ITaskAssignmentService _service;

        public TaskAssignmentController(ITaskAssignmentService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string taskId)
        {
            var result = await _service.GetByTaskIdAsync(taskId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all task assignments successfully",
                Data = result
            });
        }

        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetByAccountId(string taskId, int accountId)
        {
            try
            {
                var result = await _service.GetByAccountIdAsync(accountId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task assignments retrieved successfully for account",
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
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string taskId, int id)
        {
            try
            {
                var assignment = await _service.GetByIdAsync(id);
                if (assignment.TaskId != taskId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID does not match." });

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Task assignment retrieved successfully",
                    Data = assignment
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(string taskId, [FromBody] TaskAssignmentRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (request.TaskId != taskId)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID in request does not match URL." });

            try
            {
                var result = await _service.CreateTaskAssignment(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task assignment created successfully",
                    Data = result
                });
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
                    Message = $"Error creating task assignment: {ex.Message}"
                });
            }
        }



        [HttpPost("quick")]
        public async Task<IActionResult> CreateQuick(string taskId, [FromBody] TaskAssignmentQuickRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateTaskAssignmentQuick(taskId, request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task assignment created successfully",
                    Data = result
                });
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
                    Message = $"Error creating task assignment: {ex.Message}"
                });
            }
        }


        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk(string taskId, [FromBody] List<TaskAssignmentRequestDTO> requests)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            if (requests == null || !requests.Any())
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "List of task assignments cannot be null or empty." });
            }

            try
            {
                foreach (var request in requests)
                {
                    if (request.TaskId != taskId)
                    {
                        return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID in request does not match URL." });
                    }
                }

                var result = await _service.CreateListTaskAssignment(requests);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Task assignments created successfully",
                    Data = result
                });
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
                    Message = $"Error creating task assignments: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string taskId, int id, [FromBody] TaskAssignmentRequestDTO request)
        {
            try
            {
                var assignment = await _service.GetByIdAsync(id);
                if (assignment.TaskId != taskId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID does not match." });

                var updated = await _service.UpdateTaskAssignment(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task assignment updated successfully",
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
                    Message = $"Error updating task assignment: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string taskId, int id)
        {
            try
            {
                var assignment = await _service.GetByIdAsync(id);
                if (assignment.TaskId != taskId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID does not match." });

                await _service.DeleteTaskAssignment(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task assignment deleted successfully"
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
                    Message = $"Error deleting task assignment: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string taskId, int id, [FromBody] string status)
        {
            try
            {
                var updated = await _service.ChangeStatus(id, status);
                if (updated.TaskId != taskId)
                    return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Task ID does not match." });

                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task assignment status updated successfully",
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
                    Message = $"Error updating task assignment status: {ex.Message}"
                });
            }
        }
    }
}
