using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectPosition
{
    public int Id { get; set; }

    public int ProjectMemberId { get; set; }

    public string Position { get; set; } = null!;

    public DateTime AssignedAt { get; set; }

    public virtual ProjectMember ProjectMember { get; set; } = null!;
}
