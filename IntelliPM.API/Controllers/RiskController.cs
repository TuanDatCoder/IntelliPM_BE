using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs;
using IntelliPM.Services.RiskServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskController : ControllerBase
    {
        private readonly IRiskService _riskService;

        public RiskController(IRiskService riskService)
        {
            _riskService = riskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _riskService.GetAllRisksAsync();
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched all risks successfully",
                Data = result
            });
        }

        [HttpGet("by-project")]
        public async Task<IActionResult> GetByProjectId([FromQuery] int projectId)
        {
            var result = await _riskService.GetByProjectIdAsync(projectId);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched risks by project",
                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _riskService.GetByIdAsync(id);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Fetched risk detail successfully",
                Data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiskRequestDTO request)
        {
            await _riskService.AddAsync(request);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Created risk successfully"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RiskRequestDTO request)
        {
            await _riskService.UpdateAsync(id, request);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Updated risk successfully"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _riskService.DeleteAsync(id);
            return Ok(new ApiResponseDTO
            {
                IsSuccess = true,
                Code = 200,
                Message = "Deleted risk successfully"
            });
        }
    }

}
