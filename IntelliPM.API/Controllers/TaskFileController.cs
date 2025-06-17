using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Services.TaskCheckListServices;
using IntelliPM.Services.TaskFileServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class TaskFileController : ControllerBase
    {
        private readonly ITaskFileService _service;

        public TaskFileController(ITaskFileService service)
        {
            _service = service;
        }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] TaskFileRequestDTO request)
        {
            var result = await _service.UploadTaskFileAsync(request);
            return Ok(result);
        }

        [HttpGet("by-task/{taskId}")]
        public async Task<IActionResult> GetFilesByTaskId(int taskId)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteTaskFileAsync(id);
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
