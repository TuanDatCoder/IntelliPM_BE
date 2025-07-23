using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.DTOs.RiskSolution.Response;
using IntelliPM.Services.RiskSolutionServices;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public async Task<ActionResult<RiskSolutionResponseDTO>> Create([FromBody] RiskSolutionRequestDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet("{riskId}")]
        public async Task<ActionResult<RiskSolutionResponseDTO>> GetByRiskId(int riskId)
        {
            var result = await _service.GetListByRiskIdAsync(riskId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RiskSolutionResponseDTO>> Update([FromRoute] int id, [FromBody] RiskSolutionRequestDTO dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

    }
}
