using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Lock
{
    public string Resource { get; set; } = null!;

    public int Updatecount { get; set; }

    public DateTime? Acquired { get; set; }
}
