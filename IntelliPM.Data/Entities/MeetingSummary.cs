using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingSummary
{
    public int MeetingTranscriptId { get; set; }

    public string SummaryText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual MeetingTranscript MeetingTranscript { get; set; } = null!;
}
