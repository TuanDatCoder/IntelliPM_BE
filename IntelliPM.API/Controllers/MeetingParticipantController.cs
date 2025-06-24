using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Services.MeetingParticipantServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IntelliPM.API.Controllers
{
    [ApiController]
    [Route("api/meeting-participants")]
    public class MeetingParticipantController : ControllerBase
    {
        private readonly IMeetingParticipantService _service;

        public MeetingParticipantController(IMeetingParticipantService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MeetingParticipantRequestDTO request)
        {
            var result = await _service.CreateParticipant(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetParticipantById(id);
            return Ok(result);
        }

        [HttpGet("meeting/{meetingId}")]
        public async Task<IActionResult> GetByMeetingId(int meetingId)
        {
            var result = await _service.GetParticipantsByMeetingId(meetingId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MeetingParticipantRequestDTO request)
        {
            var result = await _service.UpdateParticipant(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteParticipant(id);
            return NoContent();
        }


    }
}