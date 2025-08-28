using System;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Request
{
    public class UpdateMeetingTranscriptRequestDTO
    {
        public int MeetingId { get; set; }
        public string TranscriptText { get; set; } = string.Empty;

        // optional (ghi lý do, ai sửa)
        public string? EditReason { get; set; }
        public int? EditedByAccountId { get; set; }

        // optional: optimistic concurrency (hash client đang giữ)
        public string? IfMatchHash { get; set; }
    }
}
