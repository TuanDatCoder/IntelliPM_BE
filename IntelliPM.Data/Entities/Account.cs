using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Role { get; set; }

    public string? Position { get; set; }

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Status { get; set; }

    public string? Address { get; set; }

    public string? Picture { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public virtual ICollection<ActivityLog> ActivityLog { get; set; } = new List<ActivityLog>();

    public virtual ICollection<ChangeRequest> ChangeRequest { get; set; } = new List<ChangeRequest>();

    public virtual ICollection<Document> DocumentApprover { get; set; } = new List<Document>();

    public virtual ICollection<DocumentComment> DocumentComment { get; set; } = new List<DocumentComment>();

    public virtual ICollection<Document> DocumentCreatedByNavigation { get; set; } = new List<Document>();

    public virtual ICollection<DocumentExportFile> DocumentExportFile { get; set; } = new List<DocumentExportFile>();

    public virtual ICollection<DocumentPermission> DocumentPermission { get; set; } = new List<DocumentPermission>();

    public virtual ICollection<Document> DocumentUpdatedByNavigation { get; set; } = new List<Document>();

    public virtual ICollection<Epic> EpicAssignedByNavigation { get; set; } = new List<Epic>();

    public virtual ICollection<EpicComment> EpicComment { get; set; } = new List<EpicComment>();

    public virtual ICollection<Epic> EpicReporter { get; set; } = new List<Epic>();

    public virtual ICollection<MeetingDocument> MeetingDocument { get; set; } = new List<MeetingDocument>();

    public virtual ICollection<MeetingLog> MeetingLog { get; set; } = new List<MeetingLog>();

    public virtual ICollection<MeetingParticipant> MeetingParticipant { get; set; } = new List<MeetingParticipant>();

    public virtual ICollection<MeetingRescheduleRequest> MeetingRescheduleRequestPm { get; set; } = new List<MeetingRescheduleRequest>();

    public virtual ICollection<MeetingRescheduleRequest> MeetingRescheduleRequestRequester { get; set; } = new List<MeetingRescheduleRequest>();

    public virtual ICollection<MilestoneFeedback> MilestoneFeedback { get; set; } = new List<MilestoneFeedback>();

    public virtual ICollection<Notification> Notification { get; set; } = new List<Notification>();

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    public virtual ICollection<ProjectMember> ProjectMember { get; set; } = new List<ProjectMember>();

    public virtual ICollection<RecipientNotification> RecipientNotification { get; set; } = new List<RecipientNotification>();

    public virtual ICollection<RefreshToken> RefreshToken { get; set; } = new List<RefreshToken>();

    public virtual ICollection<RiskComment> RiskComment { get; set; } = new List<RiskComment>();

    public virtual ICollection<Risk> RiskCreatedByNavigation { get; set; } = new List<Risk>();

    public virtual ICollection<RiskFile> RiskFile { get; set; } = new List<RiskFile>();

    public virtual ICollection<Risk> RiskResponsible { get; set; } = new List<Risk>();

    public virtual ICollection<Subtask> SubtaskAssignedByNavigation { get; set; } = new List<Subtask>();

    public virtual ICollection<SubtaskComment> SubtaskComment { get; set; } = new List<SubtaskComment>();

    public virtual ICollection<Subtask> SubtaskReporter { get; set; } = new List<Subtask>();

    public virtual ICollection<TaskAssignment> TaskAssignment { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskComment> TaskComment { get; set; } = new List<TaskComment>();

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}
