using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class RiskSolution
{
    public int Id { get; set; }

    public int RiskId { get; set; }

    public string? MitigationPlan { get; set; }

    public string? ContingencyPlan { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Risk Risk { get; set; } = null!;
}
