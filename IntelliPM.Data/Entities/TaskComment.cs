using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskComment
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public int AccountId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
