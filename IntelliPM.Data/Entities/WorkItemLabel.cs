using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class WorkItemLabel
{
    public int Id { get; set; }

    public int LabelId { get; set; }

    public string? TaskId { get; set; }

    public string? EpicId { get; set; }

    public string? SubtaskId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Epic? Epic { get; set; }

    public virtual Label Label { get; set; } = null!;

    public virtual Subtask? Subtask { get; set; }

    public virtual Tasks? Task { get; set; }
}
