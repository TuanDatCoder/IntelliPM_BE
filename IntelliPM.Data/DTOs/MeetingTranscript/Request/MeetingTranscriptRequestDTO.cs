using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Request
{
    public class MeetingTranscriptRequestDTO
    {
        public int MeetingId { get; set; }
        public IFormFile AudioFile { get; set; } // File âm thanh ghi lại cuộc họp
    }
}
