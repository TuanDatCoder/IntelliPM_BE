using IntelliPM.Services.TaskServices;
using IntelliPM.Services.WorkLogServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkLogController : ControllerBase
    {
        private readonly IWorkLogService _service;

        public WorkLogController(IWorkLogService service)
        {
            _service = service;
        }

        [HttpPost("worklog/daily-generate")]
        public async Task<IActionResult> GenerateWorkLogs()
        {
            await _service.GenerateDailyWorkLogsAsync();
            return Ok("Work logs generated");
        }
    }
}
