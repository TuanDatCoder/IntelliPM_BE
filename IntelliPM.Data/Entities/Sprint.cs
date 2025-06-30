using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Sprint
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Goal { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Epic> Epic { get; set; } = new List<Epic>();

    public virtual ICollection<Milestone> Milestone { get; set; } = new List<Milestone>();

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<Subtask> Subtask { get; set; } = new List<Subtask>();

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}
