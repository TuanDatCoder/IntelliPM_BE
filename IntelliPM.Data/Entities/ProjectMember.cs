using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectMember
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int ProjectId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime InvitedAt { get; set; }

    public string? Status { get; set; }

    public decimal? HourlyRate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Epic> Epic { get; set; } = new List<Epic>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<ProjectPosition> ProjectPosition { get; set; } = new List<ProjectPosition>();

    public virtual ICollection<Subtask> Subtask { get; set; } = new List<Subtask>();

    public virtual ICollection<TaskAssignment> TaskAssignment { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}
