using System;

namespace IntelliPM.Data.DTOs.MeetingDocument
{
    public class MeetingDocumentResponseDTO
    {
        public int MeetingId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? FileUrl { get; set; }
        public bool IsActive { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}