using System;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Request
{
    public class RestoreTranscriptRequestDTO
    {
        public int MeetingId { get; set; }
        public string FileName { get; set; } = string.Empty; // tên snapshot json trong history folder
        public string? Reason { get; set; }
        public int? EditedByAccountId { get; set; }
    }
}
