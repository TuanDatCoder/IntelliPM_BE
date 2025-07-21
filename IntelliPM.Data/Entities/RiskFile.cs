using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class RiskFile
{
    public int Id { get; set; }

    public int RiskId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Risk Risk { get; set; } = null!;

    public virtual Account UploadedByNavigation { get; set; } = null!;
}
