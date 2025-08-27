using System;

namespace IntelliPM.Data.DTOs.MeetingSummary.Request
{
    public class UpdateMeetingSummaryRequestDTO
    {
        public int MeetingTranscriptId { get; set; }
        public string SummaryText { get; set; } = string.Empty;

        // optional (ghi ly do, ai sua)
        public string? EditReason { get; set; }
        public int? EditedByAccountId { get; set; }

        // optional: optimistic concurrency (hash client dang giu)
        public string? IfMatchHash { get; set; }
    }
}