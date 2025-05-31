using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskDependency
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int LinkedFrom { get; set; }

    public int LinkedTo { get; set; }

    public string? Type { get; set; }

    public virtual Tasks LinkedFromNavigation { get; set; } = null!;

    public virtual Tasks LinkedToNavigation { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
