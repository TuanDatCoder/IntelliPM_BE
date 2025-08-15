using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Data.Entities;
using IntelliPM.Data.DTOs.Subtask.Request;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SubtaskController : ControllerBase
    {
        private readonly ISubtaskService _service;

        public SubtaskController(ISubtaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllSubtaskList();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = (int)HttpStatusCode.OK,
                Message = "View all Subtask successfully",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var taskCheckList = await _service.GetSubtaskById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Subtask retrieved successfully",
                    Data = taskCheckList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubtaskRequest2DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }
            try
            {
                var result = await _service.CreateSubtask(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Subtask created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating Subtask: {ex.Message}"
                });
            }
        }

        [HttpPost("create2")]
        public async Task<IActionResult> Create2([FromBody] SubtaskRequest2DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "Invalid request data" });
            }
            try
            {
                var result = await _service.Create2Subtask(request);
                return StatusCode(201, new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Subtask created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error creating Subtask: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SubtaskRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateSubtask(id, request);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask updated successfully",
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
                    Message = $"Error updating Subtask: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}/detailed")]
        public async Task<IActionResult> GetByIdDetailed(string id)
        {
            try
            {
                var subtask = await _service.GetSubtaskByIdDetailed(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Subtask detailed retrieved successfully",
                    Data = subtask
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpGet("by-task/{taskId}/detailed")]
        public async Task<IActionResult> GetSubtaskByTaskIdDetailed(string taskId)
        {
            try
            {
                var subtasks = await _service.GetSubtaskByTaskIdDetailed(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Subtasks detailed retrieved successfully",
                    Data = subtasks
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] SubtaskRequest3DTO dto)
        {
            try
            {
                var updated = await _service.ChangeSubtaskStatus(id, dto.Status, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask status updated successfully",
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
                    Message = $"Error updating subtask status: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteSubtask(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask deleted successfully"
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
                    Message = $"Error deleting Subtask: {ex.Message}"
                });
            }
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetSubtaskByTaskId(string taskId)
        {
            try
            {
                var files = await _service.GetSubtaskByTaskIdAsync(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved Subtask successfully.",
                    Data = files
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving Subtask: {ex.Message}"
                });
            }
        }

        [HttpPost("save-from-preview")]
        public async Task<IActionResult> SaveGeneratedSubtasks([FromBody] List<SubtaskRequest2DTO> selected)
        {
            if (selected == null || selected.Count == 0)
                return BadRequest(new ApiResponseDTO { IsSuccess = false, Code = 400, Message = "No subtasks selected" });

            try
            {
                var result = await _service.SaveGeneratedSubtasks(selected);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 201,
                    Message = "Selected subtasks saved successfully",
                    Data = result // chứa list có Id / Key
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error saving subtasks: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/planned-hours")]
        public async Task<IActionResult> ChangePlannedHours(string id, [FromBody] decimal hours, int createdBy)
        {
            try
            {
                var updated = await _service.ChangePlannedHours(id, hours, createdBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask plannedHours updated successfully",
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
                    Message = $"Error updating subtask plannedHours: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/actual-hours")]
        public async Task<IActionResult> ChangeActualHours(string id, [FromBody] decimal hours, int createdBy)
        {
            try
            {
                var updated = await _service.ChangeActualHours(id, hours, createdBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask actualHours updated successfully",
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
                    Message = $"Error updating subtask actualHours: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}/full-detailed")]
        public async Task<IActionResult> GetFullDetailedById(string id)
        {
            try
            {
                var taskCheckList = await _service.GetFullSubtaskById(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Subtask retrieved successfully",
                    Data = taskCheckList
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponseDTO { IsSuccess = false, Code = 404, Message = ex.Message });
            }
        }

    }
}
