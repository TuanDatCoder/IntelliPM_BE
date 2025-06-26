using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Services.MeetingServices;
using Microsoft.AspNetCore.Mvc;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meetings")]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingService _service;

        public MeetingController(IMeetingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeetingRequestDTO request)
        {
            try
            {
                var result = await _service.CreateMeeting(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Ghi chi tiết lỗi vào Console (hoặc ghi log file)
                Console.WriteLine("Error in Create: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                // Trả về lỗi chi tiết cho client
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the meeting.",
                    details = ex.Message,  // Gửi chi tiết lỗi
                    innerDetails = ex.InnerException?.Message  // Gửi inner exception (nếu có)
                });
            }
        }
        [HttpGet("account/{accountId}/schedule")]
public async Task<IActionResult> GetScheduleByAccount(int accountId)
{
    try
    {
        var result = await _service.GetMeetingsByAccount(accountId);
        return Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error in GetScheduleByAccount: " + ex.Message);
        return StatusCode(500, new { message = "An error occurred while retrieving meetings." });
    }
}


        [HttpGet("my")]
        public async Task<IActionResult> GetMyMeetings()
        {
            try
            {
                var result = await _service.GetMeetingsByUser();  // Lấy tất cả cuộc họp của người dùng
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMyMeetings: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving meetings." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MeetingRequestDTO request)
        {
            try
            {
                var result = await _service.UpdateMeeting(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Update: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the meeting." });
            }
        }

        [HttpGet("managed-by/{accountId}")]
        public async Task<IActionResult> GetManagedMeetings(int accountId)
        {
            var meetings = await _service.GetManagedMeetingsByAccount(accountId);
            return Ok(meetings);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _service.CancelMeeting(id);  // Hủy cuộc họp
                return Ok(new { message = "Meeting cancelled successfully." }); // DMMMMMMMM
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Cancel: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while cancelling the meeting." });
            }
        }

        [HttpPost("internal")]
        public async Task<IActionResult> CreateInternal([FromBody] MeetingRequestDTO request)
        {
            try
            {
                var result = await _service.CreateInternalMeeting(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateInternal: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                return StatusCode(500, new
                {
                    message = "An error occurred while creating the internal meeting.",
                    details = ex.Message,
                    innerDetails = ex.InnerException?.Message
                });
            }
        }
    }
}
