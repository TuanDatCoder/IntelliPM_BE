using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Subtask
{
    public string Id { get; set; } = null!;

    public string TaskId { get; set; } = null!;

    public int? AssignedBy { get; set; }

    public int? ReporterId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool ManualInput { get; set; }

    public bool GenerationAiInput { get; set; }

    public string? Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? SprintId { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public string? Duration { get; set; }

    public DateTime? ActualStartDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal? PercentComplete { get; set; }

    public decimal? PlannedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public decimal? RemainingHours { get; set; }

    public decimal? PlannedCost { get; set; }

    public decimal? PlannedResourceCost { get; set; }

    public decimal? ActualCost { get; set; }

    public decimal? ActualResourceCost { get; set; }

    public string? Evaluate { get; set; }

    public virtual ICollection<ActivityLog> ActivityLog { get; set; } = new List<ActivityLog>();

    public virtual Account? AssignedByNavigation { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual Account? Reporter { get; set; }

    public virtual Sprint? Sprint { get; set; }

    public virtual ICollection<SubtaskComment> SubtaskComment { get; set; } = new List<SubtaskComment>();

    public virtual ICollection<SubtaskFile> SubtaskFile { get; set; } = new List<SubtaskFile>();

    public virtual Tasks Task { get; set; } = null!;

    public virtual ICollection<WorkItemLabel> WorkItemLabel { get; set; } = new List<WorkItemLabel>();

    public virtual ICollection<WorkLog> WorkLog { get; set; } = new List<WorkLog>();
}
