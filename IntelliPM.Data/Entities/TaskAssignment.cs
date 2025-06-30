using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskAssignment
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public int AccountId { get; set; }

    public string? Status { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? PlannedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
