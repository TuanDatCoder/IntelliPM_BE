using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class TaskFile
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string UrlFile { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Tasks Task { get; set; } = null!;
}
