using IntelliPM.Data.DTOs;
using IntelliPM.Services.ProjectRecommendationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectRecommendationController : ControllerBase
    {
        private readonly IProjectRecommendationService _service;

        public ProjectRecommendationController(IProjectRecommendationService service)
        {
            _service = service;
        }

        [HttpGet("ai-recommendations")]
        public async Task<IActionResult> GetAIRecommendations([FromQuery] string projectKey)
        {
            try
            {
                var result = await _service.GenerateProjectRecommendationsAsync(projectKey);
                return Ok(new ApiResponseDTO
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "Success",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = $"Internal Server Error: {ex.Message}",
                    Data = null
                });
            }
        }

    }
}
