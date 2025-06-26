using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingRescheduleRequest
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public int RequesterId { get; set; }

    public DateTime RequestedDate { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public int? PmId { get; set; }

    public DateTime? PmProposedDate { get; set; }

    public string? PmNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Meeting Meeting { get; set; } = null!;

    public virtual Account? Pm { get; set; }

    public virtual Account Requester { get; set; } = null!;
}
