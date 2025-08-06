using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Risk
{
    public int Id { get; set; }

    public int? ResponsibleId { get; set; }

    public int ProjectId { get; set; }

    public string? TaskId { get; set; }

    public string RiskScope { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public string? Type { get; set; }

    public string? GeneratedBy { get; set; }

    public string? Probability { get; set; }

    public string? ImpactLevel { get; set; }

    public string? SeverityLevel { get; set; }

    public bool IsApproved { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Account? Responsible { get; set; }

    public virtual ICollection<RiskSolution> RiskSolution { get; set; } = new List<RiskSolution>();

    public virtual Tasks? Task { get; set; }
}
