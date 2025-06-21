using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class SubtaskFile
{
    public int Id { get; set; }

    public string SubtaskId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string UrlFile { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Subtask Subtask { get; set; } = null!;
}
