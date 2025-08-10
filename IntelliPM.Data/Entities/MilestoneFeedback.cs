using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MilestoneFeedback
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public int AccountId { get; set; }

    public string FeedbackText { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Meeting Meeting { get; set; } = null!;
}
