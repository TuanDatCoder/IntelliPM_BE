using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Subtask
{
    public string Id { get; set; } = null!;

    public string TaskId { get; set; } = null!;

    public int AssignedBy { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public bool ManualInput { get; set; }

    public bool GenerationAiInput { get; set; }

    public string? Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? SprintId { get; set; }

    public virtual ICollection<ActivityLog> ActivityLog { get; set; } = new List<ActivityLog>();

    public virtual Account AssignedByNavigation { get; set; } = null!;

    public virtual Sprint? Sprint { get; set; }

    public virtual ICollection<SubtaskComment> SubtaskComment { get; set; } = new List<SubtaskComment>();

    public virtual ICollection<SubtaskFile> SubtaskFile { get; set; } = new List<SubtaskFile>();

    public virtual Tasks Task { get; set; } = null!;
}
