using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Epic
{
    public string Id { get; set; } = null!;

    public int ProjectId { get; set; }

    public int? ReporterId { get; set; }

    public int? AssignedBy { get; set; }

    public int? SprintId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Account? AssignedByNavigation { get; set; }

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual ICollection<EpicComment> EpicComment { get; set; } = new List<EpicComment>();

    public virtual ICollection<EpicFile> EpicFile { get; set; } = new List<EpicFile>();

    public virtual Project Project { get; set; } = null!;

    public virtual Account? Reporter { get; set; }

    public virtual Sprint? Sprint { get; set; }

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

    public virtual ICollection<WorkItemLabel> WorkItemLabel { get; set; } = new List<WorkItemLabel>();
}
