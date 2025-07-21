using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class RiskComment
{
    public int Id { get; set; }

    public int RiskId { get; set; }

    public int AccountId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Risk Risk { get; set; } = null!;
}
