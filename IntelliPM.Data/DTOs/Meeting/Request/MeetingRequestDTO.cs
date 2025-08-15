using IntelliPM.Common.Attributes;

namespace IntelliPM.Data.DTOs.Meeting.Request
{
    public class MeetingRequestDTO
    {
        public int ProjectId { get; set; }
        public string MeetingTopic { get; set; } = null!;
        public DateTime MeetingDate { get; set; }
        public string? MeetingUrl { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Attendees { get; set; }

        public List<int> ParticipantIds { get; set; } = new();

        [DynamicCategoryValidation("meeting_status", Required = false)]
        public string? Status { get; set; }

    }
}
