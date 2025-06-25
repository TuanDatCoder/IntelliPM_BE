namespace IntelliPM.Data.DTOs.Meeting.Response
{
    public class MeetingResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string MeetingTopic { get; set; } = null!;
        public DateTime MeetingDate { get; set; }
        public string? MeetingUrl { get; set; }
        public string? Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Attendees { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProjectName { get; set; }
    }
}

