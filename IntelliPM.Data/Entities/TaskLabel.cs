using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskLabel
{
    public int Id { get; set; }

    public int LabelId { get; set; }

    public int TaskId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Label Label { get; set; } = null!;

    public virtual Tasks Task { get; set; } = null!;
}
