using System;

namespace IntelliPM.Data.DTOs.MeetingSummary.Response
{
    public class MeetingSummaryResponseDTO
    {
        public int MeetingTranscriptId { get; set; }
        public string SummaryText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? TranscriptText { get; set; }
        public bool IsApproved { get; set; }
        public string? MeetingTopic { get; set; }
    }
}