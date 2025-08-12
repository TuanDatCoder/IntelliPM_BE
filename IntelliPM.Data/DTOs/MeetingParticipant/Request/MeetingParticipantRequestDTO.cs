using IntelliPM.Common.Attributes;
using System;

namespace IntelliPM.Data.DTOs.MeetingParticipant.Request
{
    public class MeetingParticipantRequestDTO
    {
        public int MeetingId { get; set; }
        public int AccountId { get; set; }
        public string? Role { get; set; }
        //public string? Status { get; set; }

        [DynamicCategoryValidation("meeting_participant_status", Required = false)]
        public string? Status { get; set; }
    }
}