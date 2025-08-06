using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MeetingDocument
{
    public int MeetingId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? FileUrl { get; set; }

    public bool IsActive { get; set; }

    public int AccountId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Meeting Meeting { get; set; } = null!;
}
