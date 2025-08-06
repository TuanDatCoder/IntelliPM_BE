using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectRecommendation
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string? TaskId { get; set; }

    public string Type { get; set; } = null!;

    public string Recommendation { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Tasks? Task { get; set; }
}
