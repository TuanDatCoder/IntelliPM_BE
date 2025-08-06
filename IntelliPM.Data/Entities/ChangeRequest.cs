using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ChangeRequest
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int RequestedBy { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Account RequestedByNavigation { get; set; } = null!;
}
