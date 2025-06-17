using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskDependency
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public string LinkedFrom { get; set; } = null!;

    public string LinkedTo { get; set; } = null!;

    public string? Type { get; set; }

    public virtual Tasks LinkedFromNavigation { get; set; } = null!;

    public virtual Tasks LinkedToNavigation { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
