using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingTranscript
{
    public int MeetingId { get; set; }

    public string TranscriptText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual MeetingSummary? MeetingSummary { get; set; }
}
