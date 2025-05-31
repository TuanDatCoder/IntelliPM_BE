using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingLog
{
    public int Id { get; set; }

    public int MeetingId { get; set; }

    public int AccountId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Meeting Meeting { get; set; } = null!;
}
