using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Meeting
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string MeetingTopic { get; set; } = null!;

    public DateTime MeetingDate { get; set; }

    public string? MeetingUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? Attendees { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MeetingDocument? MeetingDocument { get; set; }

    public virtual ICollection<MeetingLog> MeetingLog { get; set; } = new List<MeetingLog>();

    public virtual ICollection<MeetingParticipant> MeetingParticipant { get; set; } = new List<MeetingParticipant>();

    public virtual MeetingTranscript? MeetingTranscript { get; set; }

    public virtual ICollection<MilestoneFeedback> MilestoneFeedback { get; set; } = new List<MilestoneFeedback>();

    public virtual Project Project { get; set; } = null!;
}
