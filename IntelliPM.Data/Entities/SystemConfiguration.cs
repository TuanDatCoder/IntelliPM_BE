using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class SystemConfiguration
{
    public int Id { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string? ValueConfig { get; set; }

    public string? MinValue { get; set; }

    public string? MaxValue { get; set; }

    public string? EstimateValue { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }

    public DateTime? EffectedFrom { get; set; }

    public DateTime? EffectedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
