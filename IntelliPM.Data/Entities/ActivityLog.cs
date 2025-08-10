using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ActivityLog
{
    public int Id { get; set; }

    public int? ProjectId { get; set; }

    public string? EpicId { get; set; }

    public string? TaskId { get; set; }

    public string? SubtaskId { get; set; }

    public string RelatedEntityType { get; set; } = null!;

    public string? RelatedEntityId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? FieldChanged { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Message { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RiskKey { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual Epic? Epic { get; set; }

    public virtual Risk? RiskKeyNavigation { get; set; }

    public virtual Subtask? Subtask { get; set; }

    public virtual Tasks? Task { get; set; }
}
