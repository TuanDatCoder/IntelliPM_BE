using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskCheckList
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public bool ManualInput { get; set; }

    public bool GenerationAiInput { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Tasks Task { get; set; } = null!;
}
