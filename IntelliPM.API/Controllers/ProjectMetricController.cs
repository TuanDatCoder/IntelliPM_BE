using IntelliPM.Data.DTOs;
using IntelliPM.Services.ProjectMetricServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectMetricController : ControllerBase
    {
        private readonly IProjectMetricService _service;

        public ProjectMetricController(IProjectMetricService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Success", Data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Success", Data = result });
        }

        [HttpGet("by-project-id")]
        public async Task<IActionResult> GetByProjectId([FromQuery] int projectId)
        {
            var result = await _service.GetByProjectIdAsync(projectId);
            return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Success", Data = result });
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetProjectHealth([FromQuery] int projectId)
        {
            try
            {
                var result = await _service.GetProjectHealthAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Health retrieved",
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
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateAndSave([FromQuery] int projectId, [FromQuery] string calculatedBy)
        {
            try
            {
                var result = await _service.CalculateAndSaveMetricsAsync(projectId, calculatedBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = (int)HttpStatusCode.OK,
                    Message = "Metrics calculated and saved successfully",
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
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpPost("calculate-by-ai")]
        public async Task<IActionResult> CalculateByAI([FromQuery] int projectId)
        {
            try
            {
                var result = await _service.CalculateMetricsByAIAsync(projectId);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "AI metrics calculated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO { IsSuccess = false, Code = 500, Message = $"Internal Server Error: {ex.Message}" });
            }
        }

    }
}
