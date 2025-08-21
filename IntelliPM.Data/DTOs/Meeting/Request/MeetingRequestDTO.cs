using IntelliPM.Common.Attributes;

namespace IntelliPM.Data.DTOs.Meeting.Request
{
    public class MeetingRequestDTO
    {
        public int ProjectId { get; set; }

        [DynamicMaxLength("meeting_topic_length")]
        [DynamicMinLength("meeting_topic_length")]
        public string MeetingTopic { get; set; } = null!;
        public DateTime MeetingDate { get; set; }
        public string? MeetingUrl { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [DynamicRange("meeting_attendees")]
        public int? Attendees { get; set; }

        public List<int> ParticipantIds { get; set; } = new();

        [DynamicCategoryValidation("meeting_status", Required = false)]
        public string? Status { get; set; }

    }
}
//using IntelliPM.Common.Attributes;
//using System.ComponentModel.DataAnnotations;

//namespace IntelliPM.Data.DTOs.Meeting.Request
//{
//    public class MeetingRequestDTO
//    {
//        [Required]
//        public int ProjectId { get; set; }

//        [Required]
//        [DynamicMaxLength("meeting_topic_length")]
//        [DynamicMinLength("meeting_topic_length")]
//        public string MeetingTopic { get; set; } = null!;

//        [Required]
//        public DateTime MeetingDate { get; set; }

//        [Url] // hoặc tự làm DynamicRegex nếu muốn kiểm soát chặt hơn
//        public string? MeetingUrl { get; set; }

//        // Thời gian bắt đầu/kết thúc trong ngày (nullable để cho phép draft)
//        public DateTime? StartTime { get; set; }

//        // Validate duration dựa trên key "meeting_duration_minutes"
//        [DynamicDuration("meeting_duration_minutes")] // attr sẽ đọc StartTime/EndTime
//        public DateTime? EndTime { get; set; }

//        // Validate min/max attendees theo key "meeting_attendees"
//        [DynamicRange("meeting_attendees")]
//        public int? Attendees { get; set; }

//        // Nếu muốn Attendees khớp ParticipantIds.Count thì thêm rule ở service/validator
//        public List<int> ParticipantIds { get; set; } = new();

//        [DynamicCategoryValidation("meeting_status", Required = false)]
//        public string? Status { get; set; }
//    }
//}
