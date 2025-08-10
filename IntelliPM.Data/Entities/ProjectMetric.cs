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

    public decimal? SchedulePerformanceIndex { get; set; }

    public decimal? CostPerformanceIndex { get; set; }

    public decimal? BudgetAtCompletion { get; set; }

    public decimal? DurationAtCompletion { get; set; }

    public decimal? CostVariance { get; set; }

    public decimal? ScheduleVariance { get; set; }

    public decimal? EstimateAtCompletion { get; set; }

    public decimal? EstimateToComplete { get; set; }

    public decimal? VarianceAtCompletion { get; set; }

    public decimal? EstimateDurationAtCompletion { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsImproved { get; set; }

    public string? ImprovementSummary { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public virtual Project Project { get; set; } = null!;
}
