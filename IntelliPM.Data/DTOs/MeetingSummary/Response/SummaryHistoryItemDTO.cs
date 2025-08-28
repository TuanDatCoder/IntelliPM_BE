using System;

namespace IntelliPM.Data.DTOs.MeetingSummary.Response
{
    public class SummaryHistoryItemDTO
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime TakenAtUtc { get; set; }
    }
}