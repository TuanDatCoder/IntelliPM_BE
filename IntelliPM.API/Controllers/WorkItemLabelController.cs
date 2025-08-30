using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkItemLabelController : ControllerBase
    {
        private readonly IWorkItemLabelService _service;

        public WorkItemLabelController(IWorkItemLabelService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid page or page size" });
            var result = await _service.GetAllWorkItemLabelAsync(page, pageSize);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all work item labels successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var workItemLabel = await _service.GetWorkItemLabelById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Work item label retrieved successfully",
                    Data = workItemLabel
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkItemLabelRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }

            try
            {
                var result = await _service.CreateWorkItemLabel(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Work item label created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating work item label: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkItemLabelRequestDTO request)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                var updated = await _service.UpdateWorkItemLabel(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Work item label updated successfully",
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
                    Message = $"Error updating work item label: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid ID" });
            try
            {
                await _service.DeleteWorkItemLabel(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Work item label deleted successfully"
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
                    Message = $"Error deleting work item label: {ex.Message}"
                });
            }
        }

        [HttpGet("by-epic/{epicId?}")]
        public async Task<IActionResult> GetByEpicId(string? epicId)
        {
            var result = await _service.GetByEpicIdAsync(epicId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Work item labels retrieved by epic ID successfully",
                Data = result
            });
        }

        [HttpGet("by-subtask/{subtaskId?}")]
        public async Task<IActionResult> GetBySubtaskId(string? subtaskId)
        {
            var result = await _service.GetBySubtaskIdAsync(subtaskId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Work item labels retrieved by subtask ID successfully",
                Data = result
            });
        }

        [HttpGet("by-task/{taskId?}")]
        public async Task<IActionResult> GetByTaskId(string? taskId)
        {
            var result = await _service.GetByTaskIdAsync(taskId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "Work item labels retrieved by task ID successfully",
                Data = result
            });
        }
    }
}
