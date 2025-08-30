using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskFileServices;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Services.SubtaskFileServices;
using IntelliPM.Data.DTOs.SubtaskFile.Request;
using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubtaskFileController : ControllerBase
    {
        private readonly ISubtaskFileService _service;

        public SubtaskFileController(ISubtaskFileService service)
        {
            _service = service;
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] SubtaskFileRequestDTO request)
        {
            var result = await _service.UploadSubtaskFileAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpGet("by-subtask/{subtaskId}")]
        public async Task<IActionResult> GetFilesBySubtask(string subtaskId)
        {
            try
            {
                var files = await _service.GetFilesBySubtaskIdAsync(subtaskId);
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
        public async Task<IActionResult> Delete(int id, DeleteSubtaskFileRequestDTO dto)
        {
            try
            {
                await _service.DeleteSubtaskFileAsync(id, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Subtask file deleted successfully"
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
                    Message = $"Error deleting subtask file: {ex.Message}"
                });
            }
        }
    }
}
