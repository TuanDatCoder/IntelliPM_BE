using IntelliPM.Data.DTOs.TaskFile.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.RiskFileServices;
using Microsoft.AspNetCore.Mvc;
using IntelliPM.Data.DTOs.RiskFile.Request;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskFileController : ControllerBase
    {
        private readonly IRiskFileService _service;

        public RiskFileController(IRiskFileService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] RiskFileRequestDTO request)
        {
            var result = await _service.UploadRiskFileAsync(request);
            return Ok(result);
        }

        [HttpGet("by-risk/{riskId}")]
        public async Task<IActionResult> GetFilesByRiskId(int riskId)
        {
            try
            {
                var files = await _service.GetByRiskIdAsync(riskId);
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
                await _service.DeleteRiskFileAsync(id);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Risk file deleted successfully"
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
                    Message = $"Error deleting risk file: {ex.Message}"
                });
            }
        }
    }
}
