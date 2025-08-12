using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectMetricHistory
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string MetricKey { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime RecordedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
}
