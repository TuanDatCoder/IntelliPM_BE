using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Milestone
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public int ProjectId { get; set; }

    public int? SprintId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Sprint? Sprint { get; set; }
}
