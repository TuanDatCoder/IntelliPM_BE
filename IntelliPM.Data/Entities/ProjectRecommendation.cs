using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectRecommendation
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Type { get; set; } = null!;

    public string Recommendation { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Details { get; set; }

    public string? SuggestedChanges { get; set; }

    public virtual Project Project { get; set; } = null!;
}
