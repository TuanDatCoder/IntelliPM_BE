using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Label
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? ColorCode { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<WorkItemLabel> WorkItemLabel { get; set; } = new List<WorkItemLabel>();
}
