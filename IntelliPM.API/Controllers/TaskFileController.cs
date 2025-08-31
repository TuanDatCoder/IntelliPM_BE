using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Services.TaskFileServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskFileController : ControllerBase
    {
        private readonly ITaskFileService _service;

        public TaskFileController(ITaskFileService service)
        {
            _service = service;
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] TaskFileRequestDTO request)
        {
            var result = await _service.UploadTaskFileAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetFilesByTaskId(string taskId)
        {
            try
            {
                var files = await _service.GetFilesByTaskIdAsync(taskId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Retrieved files successfully.",
                    Data = files
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Error retrieving files: {ex.Message}"
                });
            }
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, DeleteTaskFileRequestDTO dto)
        {
            try
            {
                await _service.DeleteTaskFileAsync(id, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Task file deleted successfully"
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
                    Message = $"Error deleting task file: {ex.Message}"
                });
            }
        }
    }
}
