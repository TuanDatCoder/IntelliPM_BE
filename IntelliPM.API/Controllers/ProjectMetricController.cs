using IntelliPM.Data.DTOs;
using IntelliPM.Services.ProjectMetricServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _service.GetProjectHealthAsync(projectId);
            return Ok(new ApiResponseDTO { IsSuccess = true, Code = 200, Message = "Health retrieved", Data = result });
        }
    }
}
