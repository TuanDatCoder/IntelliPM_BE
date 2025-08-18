using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class DocumentPermission
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int AccountId { get; set; }

    public string PermissionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
