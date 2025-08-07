using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class DocumentExportFile
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public string ExportedFileUrl { get; set; } = null!;

    public DateTime ExportedAt { get; set; }

    public int ExportedBy { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Account ExportedByNavigation { get; set; } = null!;
}
