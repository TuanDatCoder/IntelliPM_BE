using System;

namespace IntelliPM.Data.DTOs.MeetingSummary.Request
{
    public class MeetingSummaryRequestDTO
    {
        public int MeetingTranscriptId { get; set; }
        public string SummaryText { get; set; } = null!;
    }
}