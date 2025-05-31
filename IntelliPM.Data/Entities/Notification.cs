using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public string Type { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<RecipientNotification> RecipientNotification { get; set; } = new List<RecipientNotification>();
}
