using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class WorkLog
{
    public int Id { get; set; }

    public string? TaskId { get; set; }

    public string? SubtaskId { get; set; }

    public DateTime LogDate { get; set; }

    public decimal? Hours { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Subtask? Subtask { get; set; }

    public virtual Tasks? Task { get; set; }
}
