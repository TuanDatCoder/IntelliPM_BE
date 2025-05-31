using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskComment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Tasks Task { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
