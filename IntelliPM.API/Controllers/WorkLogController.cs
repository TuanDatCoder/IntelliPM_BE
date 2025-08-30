using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.WorkLog.Request;
using IntelliPM.Services.TaskServices;
using IntelliPM.Services.WorkLogServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkLogController : ControllerBase
    {
        private readonly IWorkLogService _service;

        public WorkLogController(IWorkLogService service)
        {
            _service = service;
        }

        [HttpPost("worklog/daily-generate")]
        public async Task<IActionResult> GenerateWorkLogs()
        {
            try
            {
                await _service.GenerateDailyWorkLogsAsync();
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Work logs generated"
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
                    Message = $"Error viewing workLog: {ex.Message}"
                });
            }

        }

        [HttpGet("by-task-or-subtask")]
        public async Task<IActionResult> GetByTaskOrSubtask([FromQuery] string? taskId, [FromQuery] string? subtaskId)
        {
            try 
            {
                var result = await _service.GetWorkLogsByTaskOrSubtaskAsync(taskId, subtaskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "View worklog successfully",
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
                    Message = $"Error viewing workLog: {ex.Message}"
                });
            }
        }

        [HttpPatch("{id}/hours")]
        public async Task<IActionResult> ChangeWorkLogHours(int id, [FromBody] decimal hours)
        {
            try
            {
                var updated = await _service.ChangeWorkLogHoursAsync(id, hours);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "WorkLog hours updated successfully",
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
                    Message = $"Error updating workLog hours: {ex.Message}"
                });
            }
        }

        [HttpPut("change-multiple-hours")]
        public async Task<IActionResult> ChangeMultipleHours([FromBody] Dictionary<int, decimal> updates)
        {
            try
            {
                var result = await _service.ChangeMultipleWorkLogHoursAsync(updates);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "WorkLog hours updated successfully",
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
                    Message = $"Error updating workLog hours: {ex.Message}"
                });
            }
        }

        [HttpPut("update-by-accounts")]
        public async Task<IActionResult> UpdateWorkLogByAccounts([FromBody] UpdateWorkLogsByAccountsDTO dto)
        {
            try
            {
                var success = await _service.UpdateWorkLogsByAccountsAsync(dto);
                return success ? Ok(new { isSuccess = true }) : BadRequest(new { isSuccess = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = ex.Message });
            }
        }


    }
}
