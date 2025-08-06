using System;

namespace IntelliPM.Data.DTOs.MeetingParticipant.Response
{
    public class MeetingParticipantResponseDTO
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int AccountId { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? FullName { get; set; }
    }
}
