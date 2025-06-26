using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Data.DTOs.MilestoneFeedback.Request
{
    public class MilestoneFeedbackRequestDTO
    {
        [Required(ErrorMessage = "Meeting ID is required")]
        public int MeetingId { get; set; }

        [Required(ErrorMessage = "Account ID is required")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Feedback text is required")]
        public string FeedbackText { get; set; } = null!;

        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = null!;
    }
}