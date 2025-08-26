using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MeetingTranscript.Response
{
    public class TranscriptHistoryItemDTO
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime TakenAtUtc { get; set; }
    }
}

