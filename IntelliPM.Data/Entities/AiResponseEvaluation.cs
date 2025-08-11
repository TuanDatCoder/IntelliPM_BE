using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class AiResponseEvaluation
{
    public int Id { get; set; }

    public int AiResponseId { get; set; }

    public int AccountId { get; set; }

    public int Rating { get; set; }

    public string? Feedback { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual AiResponseHistory AiResponse { get; set; } = null!;
}
