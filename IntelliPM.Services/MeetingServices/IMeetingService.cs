using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingServices
{
    public interface IMeetingService
    {
        // Method to create a meeting
        Task<MeetingResponseDTO> CreateMeeting(MeetingRequestDTO dto);

        // Method to get all meetings for the user (no token required)
        Task<List<MeetingResponseDTO>> GetMeetingsByUser();

        // Method to update an existing meeting
        Task<MeetingResponseDTO> UpdateMeeting(int id, MeetingRequestDTO dto);

        // Method to cancel a meeting
        Task CancelMeeting(int id);

        Task<List<MeetingResponseDTO>> GetMeetingsByAccount(int accountId);

        Task<List<MeetingResponseDTO>> GetManagedMeetingsByAccount(int accountId);

        Task<MeetingResponseDTO> CreateInternalMeeting(MeetingRequestDTO dto);

        Task CompleteMeeting(int meetingId);

        Task<List<object>> GetParticipantsWithMeetingConflict(DateTime date, DateTime startTime, DateTime endTime);

    }
}

