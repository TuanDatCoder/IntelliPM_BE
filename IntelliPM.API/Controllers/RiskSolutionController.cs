using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Services.RiskSolutionServices;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskSolutionController : ControllerBase
    {
        private readonly IRiskSolutionService _service;

        public RiskSolutionController(IRiskSolutionService service)
        {
            _service = service;
        }

        [HttpGet("by-risk/{riskId}")]
        public async Task<IActionResult> GetByRiskId(int riskId)
        {
            try
            {
                var list = await _service.GetByRiskIdAsync(riskId);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Get risk solution successfully",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get risk solution: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiskSolutionRequestDTO dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Create risk solution successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create risk solution: {ex.Message}");
            }
        }

        [HttpPatch("{id}/contigency-plan")]
        public async Task<IActionResult> UpdateContigencyPlan(int id, [FromBody] string impactLevel, int createdBy)
        {
            try
            {
                var updated = await _service.UpdateContigencyPlanAsync(id, impactLevel, createdBy);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update contigency plan successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update contigency plan: {ex.Message}");
            }
        }

        [HttpPatch("{id}/mitigation-plan")]
        public async Task<IActionResult> UpdateMitigationPlan(int id, [FromBody] string mitigationPlan, int createdBy)
        {
            try
            {
                var updated = await _service.UpdateMitigationPlanAsync(id, mitigationPlan, createdBy);
                if (updated == null)
                    return NotFound($"Risk with ID {id} not found");

                return Ok(new
                {
                    isSuccess = true,
                    code = 200,
                    message = "Update mitigation plan successfully",
                    data = updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update mitigation plan: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, int createdBy)
        {
            try
            {
                await _service.DeleteRiskSolution(id, createdBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Deleted risk solution successfully"
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
                    Message = $"Error deleting risk solution: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}/mitigation-plan")]
        public async Task<IActionResult> DeleteMitigationPlan(int id, int createdBy)
        {
            try
            {
                await _service.DeleteMitigationPlan(id, createdBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Deleted risk solution successfully"
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
                    Message = $"Error deleting risk solution: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}/contingency-plan")]
        public async Task<IActionResult> DeleteContingencyPlan(int id, int createdBy)
        {
            try
            {
                await _service.DeleteContingencyPlan(id, createdBy);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Deleted risk solution successfully"
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
                    Message = $"Error deleting risk solution: {ex.Message}"
                });
            }
        }
    }
}
