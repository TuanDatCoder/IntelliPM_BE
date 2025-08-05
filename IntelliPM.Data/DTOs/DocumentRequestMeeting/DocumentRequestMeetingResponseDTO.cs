using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentRequestMeeting
{
    public class DocumentRequestMeetingResponseDTO
    {

        public int Id { get; set; }
        public string FileUrl { get; set; } = null!;
        public int TeamLeaderId { get; set; }
        public int ProjectManagerId { get; set; }
        public string Status { get; set; } = null!;
        public string? Reason { get; set; }
        public int FeedbackId { get; set; }
        public bool? SentToClient { get; set; }
        public bool? ClientViewed { get; set; }
        public bool? ClientApproved { get; set; }
        public DateTime? ClientApprovalTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
