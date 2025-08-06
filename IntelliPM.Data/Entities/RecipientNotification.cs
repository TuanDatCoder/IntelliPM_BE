using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class RecipientNotification
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int NotificationId { get; set; }

    public string? Status { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Notification Notification { get; set; } = null!;
}
