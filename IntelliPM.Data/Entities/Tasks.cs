using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Tasks
{
    public string Id { get; set; } = null!;

    public int ReporterId { get; set; }

    public int ProjectId { get; set; }

    public string? EpicId { get; set; }

    public int? SprintId { get; set; }

    public string? Type { get; set; }

    public bool ManualInput { get; set; }

    public bool GenerationAiInput { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public string? Duration { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal? PercentComplete { get; set; }

    public decimal? PlannedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public decimal? PlannedCost { get; set; }

    public decimal? PlannedResourceCost { get; set; }

    public decimal? ActualCost { get; set; }

    public decimal? ActualResourceCost { get; set; }

    public decimal? RemainingHours { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Priority { get; set; }

    public string? Evaluate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ActivityLog> ActivityLog { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual Epic? Epic { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Account Reporter { get; set; } = null!;

    public virtual ICollection<Risk> Risk { get; set; } = new List<Risk>();

    public virtual Sprint? Sprint { get; set; }

    public virtual ICollection<Subtask> Subtask { get; set; } = new List<Subtask>();

    public virtual ICollection<TaskAssignment> TaskAssignment { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskComment> TaskComment { get; set; } = new List<TaskComment>();

    public virtual ICollection<TaskFile> TaskFile { get; set; } = new List<TaskFile>();

    public virtual ICollection<WorkItemLabel> WorkItemLabel { get; set; } = new List<WorkItemLabel>();

    public virtual ICollection<WorkLog> WorkLog { get; set; } = new List<WorkLog>();
}
