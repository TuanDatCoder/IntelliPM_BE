using IntelliPM.Services.ProjectMetricHistoryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectMetricHistoryController : ControllerBase
    {
        private readonly IProjectMetricHistoryService _historyService;

        public ProjectMetricHistoryController(IProjectMetricHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet("history/{projectKey}")]
        public async Task<IActionResult> GetMetricHistoryByProjectKey(string projectKey)
        {
            try
            {
                var history = await _historyService.GetByProjectKeyAsync(projectKey);
                return Ok(new { isSuccess = true, code = 200, data = history, message = "Metric history retrieved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, code = 400, message = ex.Message });
            }
        }
    }
}
