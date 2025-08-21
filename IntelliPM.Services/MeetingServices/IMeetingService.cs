using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.Entities;
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
        Task<(bool Removed, string? Reason)> RemoveParticipantAsync(int meetingId, int accountId);

        Task<List<int>> CheckMeetingConflictAsync(List<int> participantIds, DateTime date, DateTime startTime, DateTime endTime);

        Task<(List<int> Added, List<int> AlreadyIn, List<int> Conflicted, List<int> NotFound)>
    AddParticipantsAsync(int meetingId, List<int> participantIds);

    }
}

