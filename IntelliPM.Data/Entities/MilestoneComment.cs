using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class MilestoneComment
{
    public int Id { get; set; }

    public int MilestoneId { get; set; }

    public int AccountId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Milestone Milestone { get; set; } = null!;
}
