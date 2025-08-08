using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Request
{
    public class MeetingTranscriptFromUrlRequestDTO
    {
        public int MeetingId { get; set; }
        public string VideoUrl { get; set; } = null!;
    }
}
