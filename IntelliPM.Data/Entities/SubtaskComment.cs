using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class SubtaskComment
{
    public int Id { get; set; }

    public string SubtaskId { get; set; } = null!;

    public int AccountId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Subtask Subtask { get; set; } = null!;
}
