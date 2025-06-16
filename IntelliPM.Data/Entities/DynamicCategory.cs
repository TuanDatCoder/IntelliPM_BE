using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class DynamicCategory
{
    public int Id { get; set; }

    public string CategoryGroup { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }
}
