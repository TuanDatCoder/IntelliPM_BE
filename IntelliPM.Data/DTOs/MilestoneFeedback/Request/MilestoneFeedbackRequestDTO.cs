using IntelliPM.Common.Attributes;
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
        [DynamicMaxLength("milestone_feedback_text")]
        [DynamicMinLength("milestone_feedback_text")]
        public string FeedbackText { get; set; } = null!;

        [Required]
        [DynamicCategoryValidation("milestone_feedback_status", Required = true)]
        public string Status { get; set; } = null!;

    }
}