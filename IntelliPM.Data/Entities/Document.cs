using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Document
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string? TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string? Type { get; set; }

    public string? Template { get; set; }

    public string? Content { get; set; }

    public string? FileUrl { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<DocumentPermission> DocumentPermission { get; set; } = new List<DocumentPermission>();

    public virtual Project Project { get; set; } = null!;

    public virtual Tasks? Task { get; set; }

    public virtual Account? UpdatedByNavigation { get; set; }

    public string Status { get; set; } = "Draft"; // Enum: Draft, PendingApproval, Approved, Rejected

    public string? EpicId { get; set; }
    public string? SubTaskId { get; set; }

    public virtual Epic? Epic { get; set; }
    public virtual Subtask? SubTask { get; set; }


}
