using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class ProjectMetric
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string CalculatedBy { get; set; } = null!;

    public bool IsApproved { get; set; }

    public decimal? PlannedValue { get; set; }

    public decimal? EarnedValue { get; set; }

    public decimal? ActualCost { get; set; }

    public decimal? Spi { get; set; }

    public decimal? Cpi { get; set; }

    public int? DelayDays { get; set; }

    public decimal? BudgetOverrun { get; set; }

    public DateTime? ProjectedFinishDate { get; set; }

    public decimal? ProjectedTotalCost { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Project Project { get; set; } = null!;
}
