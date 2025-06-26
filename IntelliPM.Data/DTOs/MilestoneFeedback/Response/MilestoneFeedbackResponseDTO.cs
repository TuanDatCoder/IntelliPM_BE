namespace IntelliPM.Data.DTOs.MilestoneFeedback.Response
{
    public class MilestoneFeedbackResponseDTO
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int AccountId { get; set; }
        public string FeedbackText { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}