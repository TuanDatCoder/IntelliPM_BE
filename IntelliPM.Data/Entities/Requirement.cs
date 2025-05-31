using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Requirement
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Type { get; set; }

    public string? Description { get; set; }

    public string? Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
}
