using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.TaskFileServices;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Services.EpicFileServices;
using IntelliPM.Data.DTOs.EpicFile.Request;
using Microsoft.AspNetCore.Authorization;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EpicFileController : ControllerBase
    {
        private readonly IEpicFileService _service;

        public EpicFileController(IEpicFileService service)
        {
            _service = service;
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] EpicFileRequestDTO request)
        {
            var result = await _service.UploadEpicFileAsync(request);
            return Ok(result);
        }

        [Authorize(Roles = "PROJECT_MANAGER,TEAM_LEADER,TEAM_MEMBER")]
        [HttpGet("by-epic/{epicId}")]
        public async Task<IActionResult> GetFilesByEpicId(string epicId)
        {
            try
            {
                var files = await _service.GetFilesByEpicIdAsync(epicId);
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
        public async Task<IActionResult> Delete(int id, DeleteEpicFileRequestDTO dto)
        {
            try
            {
                await _service.DeleteEpicFileAsync(id, dto.CreatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Epic file deleted successfully"
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
                    Message = $"Error deleting epic file: {ex.Message}"
                });
            }
        }
    }
}
