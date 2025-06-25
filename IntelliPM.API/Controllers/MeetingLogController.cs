using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Services.MeetingLogServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/meeting-logs")]
public class MeetingLogController : ControllerBase
{
    private readonly IMeetingLogService _service;

    public MeetingLogController(IMeetingLogService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddLog([FromBody] MeetingLogRequestDTO request)
    {
        try
        {
            var result = await _service.AddLogAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in AddLog: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred while adding the log." });
        }
    }

    [HttpGet("meeting/{meetingId}")]
    public async Task<IActionResult> GetLogsByMeetingId(int meetingId)
    {
        try
        {
            var result = await _service.GetLogsByMeetingIdAsync(meetingId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetLogsByMeetingId: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred while retrieving logs." });
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetLogsByAccountId(int accountId)
    {
        try
        {
            var result = await _service.GetLogsByAccountIdAsync(accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetLogsByAccountId: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred while retrieving logs." });
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllLogs()
    {
        try
        {
            var result = await _service.GetAllLogsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in GetAllLogs: " + ex.Message);
            return StatusCode(500, new { message = "An error occurred while retrieving logs." });
        }
    }
}