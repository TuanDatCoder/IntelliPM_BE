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

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteMeeting(int id)
        {
            await _service.CompleteMeeting(id);  // Đúng tên biến rồi
            return Ok(new { message = "Meeting marked as COMPLETED" });
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

        [HttpGet("check-conflict")]
        public async Task<IActionResult> CheckMeetingConflict(
            [FromQuery] List<int> participantIds,
            [FromQuery] DateTime date,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            try
            {
                if (participantIds == null || !participantIds.Any())
                    return BadRequest(new { message = "Participant list cannot be empty." });

                var conflictingAccountIds = await _service.CheckMeetingConflictAsync(participantIds, date, startTime, endTime);

                if (conflictingAccountIds == null || !conflictingAccountIds.Any())
                    return Ok(new { message = "No participant has a meeting conflict." });

                return Ok(new
                {
                    message = "Some participants have conflicting meetings.",
                    conflictingAccountIds
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CheckMeetingConflict: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while checking meeting conflicts." });
            }
        }

        [HttpPost("{id}/participants")]
        public async Task<IActionResult> AddParticipants(int id, [FromBody] List<int> participantIds)
        {
            try
            {
                var (added, alreadyIn, conflicted, notFound) =
                    await _service.AddParticipantsAsync(id, participantIds);

                var nothingAdded = added == null || added.Count == 0;
                if (nothingAdded && ((conflicted?.Count ?? 0) > 0 || (notFound?.Count ?? 0) > 0))
                {
                    return Conflict(new
                    {
                        message = "No participant was added due to conflicts or not found.",
                        added,
                        alreadyIn,
                        conflicted,
                        notFound
                    });
                }

                return Ok(new { added, alreadyIn, conflicted, notFound });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AddParticipants: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);

                return StatusCode(500, new
                {
                    message = "An error occurred while adding participants.",
                    details = ex.Message,
                    innerDetails = ex.InnerException?.Message
                });
            }
        }

        [HttpDelete("{id}/participants/{accountId}")]
        public async Task<IActionResult> RemoveParticipant(int id, int accountId)
        {
            try
            {
                var (removed, reason) = await _service.RemoveParticipantAsync(id, accountId);
                if (!removed)
                {
                    // tuỳ lý do mà trả mã phù hợp
                    if (reason == "Meeting not found" || reason == "Participant not in meeting")
                        return NotFound(new { message = reason });
                    if (reason == "Cannot remove creator" || reason == "Meeting is CANCELLED")
                        return BadRequest(new { message = reason });

                    return StatusCode(409, new { message = reason }); // conflict mặc định
                }

                return Ok(new { message = "Participant removed successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RemoveParticipant: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while removing participant." });
            }
        }

    }
}
