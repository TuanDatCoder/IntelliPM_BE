using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class EpicFile
{
    public int Id { get; set; }

    public string EpicId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string UrlFile { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Epic Epic { get; set; } = null!;
}
