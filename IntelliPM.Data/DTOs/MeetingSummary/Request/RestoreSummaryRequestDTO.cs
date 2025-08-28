using System;

namespace IntelliPM.Data.DTOs.MeetingSummary.Request
{
    public class RestoreSummaryRequestDTO
    {
        public int MeetingTranscriptId { get; set; }
        public string FileName { get; set; } = string.Empty; // tên snapshot json trong history folder
        public string? Reason { get; set; }
        public int? EditedByAccountId { get; set; }
    }
}