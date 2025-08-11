using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class AiResponseHistory
{
    public int Id { get; set; }

    public string AiFeature { get; set; } = null!;

    public int? ProjectId { get; set; }

    public string ResponseJson { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<AiResponseEvaluation> AiResponseEvaluation { get; set; } = new List<AiResponseEvaluation>();

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual Project? Project { get; set; }
}
