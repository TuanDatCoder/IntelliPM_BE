using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class DocumentRequestMeeting
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

    public virtual MilestoneFeedback Feedback { get; set; } = null!;

    public virtual Account ProjectManager { get; set; } = null!;

    public virtual Account TeamLeader { get; set; } = null!;
}
