using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskDependency
{
    public int Id { get; set; }

    public string FromType { get; set; } = null!;

    public string LinkedFrom { get; set; } = null!;

    public string ToType { get; set; } = null!;

    public string LinkedTo { get; set; } = null!;

    public string Type { get; set; } = null!;
}
