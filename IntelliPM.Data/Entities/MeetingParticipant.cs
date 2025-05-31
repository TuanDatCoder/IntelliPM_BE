using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingParticipant
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public int AccountId { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Meeting Meeting { get; set; } = null!;
}
