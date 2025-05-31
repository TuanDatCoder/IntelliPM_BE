using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskStatusLog
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string Status { get; set; } = null!;

    public int ChangedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account ChangedByNavigation { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
