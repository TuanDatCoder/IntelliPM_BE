using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentRequestMeeting
{
    public class CreateDocumentRequestMeetingDTO
    {
        public IFormFile File { get; set; } = null!;
        public int TeamLeaderId { get; set; }
        public int ProjectManagerId { get; set; }
        public string Status { get; set; } = null!;
        public string? Reason { get; set; }
        public int FeedbackId { get; set; }
    }

}
