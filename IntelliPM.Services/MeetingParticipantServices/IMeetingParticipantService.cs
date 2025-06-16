using IntelliPM.Data.DTOs.MeetingParticipant.Request;
using IntelliPM.Data.DTOs.MeetingParticipant.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingParticipantServices
{
    public interface IMeetingParticipantService
    {
        Task<MeetingParticipantResponseDTO> CreateParticipant(MeetingParticipantRequestDTO dto);
        Task<MeetingParticipantResponseDTO> GetParticipantById(int id);
        Task<List<MeetingParticipantResponseDTO>> GetParticipantsByMeetingId(int meetingId);
        Task<MeetingParticipantResponseDTO> UpdateParticipant(int id, MeetingParticipantRequestDTO dto);
        Task DeleteParticipant(int id);
        Task CreateParticipantsForMeeting(int meetingId, int attendeesCount);
    }
}