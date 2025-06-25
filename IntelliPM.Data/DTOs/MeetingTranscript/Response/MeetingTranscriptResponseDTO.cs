using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Response
{
    public class MeetingTranscriptResponseDTO
    {
        public int MeetingId { get; set; }
        public string TranscriptText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
