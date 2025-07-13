using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Contexts;

public partial class Su25Sep490IntelliPmContext : DbContext
{
    public Su25Sep490IntelliPmContext()
    {
    }

    public Su25Sep490IntelliPmContext(DbContextOptions<Su25Sep490IntelliPmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Account { get; set; }

    public virtual DbSet<ActivityLog> ActivityLog { get; set; }

    public virtual DbSet<ChangeRequest> ChangeRequest { get; set; }

    public virtual DbSet<Document> Document { get; set; }

    public virtual DbSet<DocumentPermission> DocumentPermission { get; set; }

    public virtual DbSet<DynamicCategory> DynamicCategory { get; set; }

    public virtual DbSet<Epic> Epic { get; set; }

    public virtual DbSet<EpicComment> EpicComment { get; set; }

    public virtual DbSet<EpicFile> EpicFile { get; set; }

    public virtual DbSet<Label> Label { get; set; }

    public virtual DbSet<Meeting> Meeting { get; set; }

    public virtual DbSet<MeetingDocument> MeetingDocument { get; set; }

    public virtual DbSet<MeetingLog> MeetingLog { get; set; }

    public virtual DbSet<MeetingParticipant> MeetingParticipant { get; set; }

    public virtual DbSet<MeetingRescheduleRequest> MeetingRescheduleRequest { get; set; }

    public virtual DbSet<MeetingSummary> MeetingSummary { get; set; }

    public virtual DbSet<MeetingTranscript> MeetingTranscript { get; set; }

    public virtual DbSet<Milestone> Milestone { get; set; }

    public virtual DbSet<MilestoneFeedback> MilestoneFeedback { get; set; }

    public virtual DbSet<Notification> Notification { get; set; }

    public virtual DbSet<Project> Project { get; set; }

    public virtual DbSet<ProjectMember> ProjectMember { get; set; }

    public virtual DbSet<ProjectMetric> ProjectMetric { get; set; }

    public virtual DbSet<ProjectPosition> ProjectPosition { get; set; }

    public virtual DbSet<ProjectRecommendation> ProjectRecommendation { get; set; }

    public virtual DbSet<RecipientNotification> RecipientNotification { get; set; }

    public virtual DbSet<RefreshToken> RefreshToken { get; set; }

    public virtual DbSet<Requirement> Requirement { get; set; }

    public virtual DbSet<Risk> Risk { get; set; }

    public virtual DbSet<RiskSolution> RiskSolution { get; set; }

    public virtual DbSet<Sprint> Sprint { get; set; }

    public virtual DbSet<Subtask> Subtask { get; set; }

    public virtual DbSet<SubtaskComment> SubtaskComment { get; set; }

    public virtual DbSet<SubtaskFile> SubtaskFile { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfiguration { get; set; }

    public virtual DbSet<TaskAssignment> TaskAssignment { get; set; }

    public virtual DbSet<TaskComment> TaskComment { get; set; }

    public virtual DbSet<TaskDependency> TaskDependency { get; set; }

    public virtual DbSet<TaskFile> TaskFile { get; set; }

    public virtual DbSet<Tasks> Tasks { get; set; }

    public virtual DbSet<WorkItemLabel> WorkItemLabel { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SU25_SEP490_IntelliPM;Username=postgres;Password=12345;");

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(GetConnectionString("DefaultConnection"));



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_pkey");

            entity.ToTable("account");

            entity.HasIndex(e => e.Email, "account_email_key").IsUnique();

            entity.HasIndex(e => e.GoogleId, "account_google_id_key").IsUnique();

            entity.HasIndex(e => e.Username, "account_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .HasColumnName("google_id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Picture)
                .HasMaxLength(255)
                .HasColumnName("picture");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("activity_log_pkey");

            entity.ToTable("activity_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(100)
                .HasColumnName("action_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.FieldChanged)
                .HasMaxLength(100)
                .HasColumnName("field_changed");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RelatedEntityId)
                .HasMaxLength(255)
                .HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(100)
                .HasColumnName("related_entity_type");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("activity_log_created_by_fkey");

            entity.HasOne(d => d.Subtask).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("activity_log_subtask_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("activity_log_task_id_fkey");
        });

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("change_request_pkey");

            entity.ToTable("change_request");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.ChangeRequest)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("change_request_project_id_fkey");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.ChangeRequest)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("change_request_requested_by_fkey");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_pkey");

            entity.ToTable("document");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApproverId).HasColumnName("approver_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(1024)
                .HasColumnName("file_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Draft'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Template).HasColumnName("template");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Approver).WithMany(p => p.DocumentApprover)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("document_approver_id_fkey");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DocumentCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_created_by_fkey");

            entity.HasOne(d => d.Epic).WithMany(p => p.Document)
                .HasForeignKey(d => d.EpicId)
                .HasConstraintName("document_epic_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Document)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_project_id_fkey");

            entity.HasOne(d => d.Subtask).WithMany(p => p.Document)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("document_subtask_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Document)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("document_task_id_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.DocumentUpdatedByNavigation)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("document_updated_by_fkey");
        });

        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_permission_pkey");

            entity.ToTable("document_permission");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.PermissionType)
                .HasMaxLength(50)
                .HasColumnName("permission_type");

            entity.HasOne(d => d.Account).WithMany(p => p.DocumentPermission)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_permission_account_id_fkey");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentPermission)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("document_permission_document_id_fkey");
        });

        modelBuilder.Entity<DynamicCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dynamic_category_pkey");

            entity.ToTable("dynamic_category");

            entity.HasIndex(e => new { e.CategoryGroup, e.Name }, "dynamic_category_category_group_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryGroup)
                .HasMaxLength(100)
                .HasColumnName("category_group");
            entity.Property(e => e.Color)
                .HasMaxLength(10)
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconLink).HasColumnName("icon_link");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Label)
                .HasMaxLength(255)
                .HasColumnName("label");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OrderIndex)
                .HasDefaultValue(0)
                .HasColumnName("order_index");
        });

        modelBuilder.Entity<Epic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epic_pkey");

            entity.ToTable("epic");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.SprintId).HasColumnName("sprint_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.EpicAssignedByNavigation)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("epic_assigned_by_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Epic)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("epic_project_id_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.EpicReporter)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("epic_reporter_id_fkey");

            entity.HasOne(d => d.Sprint).WithMany(p => p.Epic)
                .HasForeignKey(d => d.SprintId)
                .HasConstraintName("epic_sprint_id_fkey");
        });

        modelBuilder.Entity<EpicComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epic_comment_pkey");

            entity.ToTable("epic_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");

            entity.HasOne(d => d.Account).WithMany(p => p.EpicComment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("epic_comment_account_id_fkey");

            entity.HasOne(d => d.Epic).WithMany(p => p.EpicComment)
                .HasForeignKey(d => d.EpicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("epic_comment_epic_id_fkey");
        });

        modelBuilder.Entity<EpicFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epic_file_pkey");

            entity.ToTable("epic_file");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UrlFile)
                .HasMaxLength(1024)
                .HasColumnName("url_file");

            entity.HasOne(d => d.Epic).WithMany(p => p.EpicFile)
                .HasForeignKey(d => d.EpicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("epic_file_epic_id_fkey");
        });

        modelBuilder.Entity<Label>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("label_pkey");

            entity.ToTable("label");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColorCode)
                .HasMaxLength(10)
                .HasColumnName("color_code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");

            entity.HasOne(d => d.Project).WithMany(p => p.Label)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("label_project_id_fkey");
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meeting_pkey");

            entity.ToTable("meeting");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attendees).HasColumnName("attendees");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.MeetingDate).HasColumnName("meeting_date");
            entity.Property(e => e.MeetingTopic)
                .HasMaxLength(255)
                .HasColumnName("meeting_topic");
            entity.Property(e => e.MeetingUrl)
                .HasMaxLength(1024)
                .HasColumnName("meeting_url");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Project).WithMany(p => p.Meeting)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_project_id_fkey");
        });

        modelBuilder.Entity<MeetingDocument>(entity =>
        {
            entity.HasKey(e => e.MeetingId).HasName("meeting_document_pkey");

            entity.ToTable("meeting_document");

            entity.Property(e => e.MeetingId)
                .ValueGeneratedNever()
                .HasColumnName("meeting_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(1024)
                .HasColumnName("file_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Account).WithMany(p => p.MeetingDocument)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_document_account_id_fkey");

            entity.HasOne(d => d.Meeting).WithOne(p => p.MeetingDocument)
                .HasForeignKey<MeetingDocument>(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_document_meeting_id_fkey");
        });

        modelBuilder.Entity<MeetingLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meeting_log_pkey");

            entity.ToTable("meeting_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MeetingId).HasColumnName("meeting_id");

            entity.HasOne(d => d.Account).WithMany(p => p.MeetingLog)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_log_account_id_fkey");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingLog)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_log_meeting_id_fkey");
        });

        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meeting_participant_pkey");

            entity.ToTable("meeting_participant");

            entity.HasIndex(e => new { e.MeetingId, e.AccountId }, "meeting_participant_meeting_id_account_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MeetingId).HasColumnName("meeting_id");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.MeetingParticipant)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_participant_account_id_fkey");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingParticipant)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_participant_meeting_id_fkey");
        });

        modelBuilder.Entity<MeetingRescheduleRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meeting_reschedule_request_pkey");

            entity.ToTable("meeting_reschedule_request");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MeetingId).HasColumnName("meeting_id");
            entity.Property(e => e.PmId).HasColumnName("pm_id");
            entity.Property(e => e.PmNote).HasColumnName("pm_note");
            entity.Property(e => e.PmProposedDate).HasColumnName("pm_proposed_date");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RequestedDate).HasColumnName("requested_date");
            entity.Property(e => e.RequesterId).HasColumnName("requester_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MeetingRescheduleRequest)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_reschedule_request_meeting_id_fkey");

            entity.HasOne(d => d.Pm).WithMany(p => p.MeetingRescheduleRequestPm)
                .HasForeignKey(d => d.PmId)
                .HasConstraintName("meeting_reschedule_request_pm_id_fkey");

            entity.HasOne(d => d.Requester).WithMany(p => p.MeetingRescheduleRequestRequester)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_reschedule_request_requester_id_fkey");
        });

        modelBuilder.Entity<MeetingSummary>(entity =>
        {
            entity.HasKey(e => e.MeetingTranscriptId).HasName("meeting_summary_pkey");

            entity.ToTable("meeting_summary");

            entity.Property(e => e.MeetingTranscriptId)
                .ValueGeneratedNever()
                .HasColumnName("meeting_transcript_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.SummaryText).HasColumnName("summary_text");

            entity.HasOne(d => d.MeetingTranscript).WithOne(p => p.MeetingSummary)
                .HasForeignKey<MeetingSummary>(d => d.MeetingTranscriptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_summary_meeting_transcript_id_fkey");
        });

        modelBuilder.Entity<MeetingTranscript>(entity =>
        {
            entity.HasKey(e => e.MeetingId).HasName("meeting_transcript_pkey");

            entity.ToTable("meeting_transcript");

            entity.Property(e => e.MeetingId)
                .ValueGeneratedNever()
                .HasColumnName("meeting_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.TranscriptText).HasColumnName("transcript_text");

            entity.HasOne(d => d.Meeting).WithOne(p => p.MeetingTranscript)
                .HasForeignKey<MeetingTranscript>(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("meeting_transcript_meeting_id_fkey");
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("milestone_pkey");

            entity.ToTable("milestone");

            entity.HasIndex(e => e.Key, "milestone_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Key)
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.SprintId).HasColumnName("sprint_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.Milestone)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestone_project_id_fkey");

            entity.HasOne(d => d.Sprint).WithMany(p => p.Milestone)
                .HasForeignKey(d => d.SprintId)
                .HasConstraintName("milestone_sprint_id_fkey");
        });

        modelBuilder.Entity<MilestoneFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("milestone_feedback_pkey");

            entity.ToTable("milestone_feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.FeedbackText).HasColumnName("feedback_text");
            entity.Property(e => e.MeetingId).HasColumnName("meeting_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.MilestoneFeedback)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestone_feedback_account_id_fkey");

            entity.HasOne(d => d.Meeting).WithMany(p => p.MilestoneFeedback)
                .HasForeignKey(d => d.MeetingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestone_feedback_meeting_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .HasColumnName("priority");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(100)
                .HasColumnName("related_entity_type");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Notification)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_created_by_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_pkey");

            entity.ToTable("project");

            entity.HasIndex(e => e.ProjectKey, "project_project_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Budget)
                .HasPrecision(15, 2)
                .HasColumnName("budget");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProjectKey)
                .HasMaxLength(10)
                .HasColumnName("project_key");
            entity.Property(e => e.ProjectType)
                .HasMaxLength(50)
                .HasColumnName("project_type");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Project)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_created_by_fkey");
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_member_pkey");

            entity.ToTable("project_member");

            entity.HasIndex(e => new { e.AccountId, e.ProjectId }, "project_member_account_id_project_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.HourlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("hourly_rate");
            entity.Property(e => e.InvitedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("invited_at");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.ProjectMember)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_member_account_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMember)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_member_project_id_fkey");
        });

        modelBuilder.Entity<ProjectMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_metric_pkey");

            entity.ToTable("project_metric");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualCost)
                .HasPrecision(15, 2)
                .HasColumnName("actual_cost");
            entity.Property(e => e.BudgetOverrun)
                .HasPrecision(15, 2)
                .HasColumnName("budget_overrun");
            entity.Property(e => e.CalculatedBy)
                .HasMaxLength(50)
                .HasColumnName("calculated_by");
            entity.Property(e => e.Cpi)
                .HasPrecision(15, 2)
                .HasColumnName("cpi");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DelayDays).HasColumnName("delay_days");
            entity.Property(e => e.EarnedValue)
                .HasPrecision(15, 2)
                .HasColumnName("earned_value");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.PlannedValue)
                .HasPrecision(15, 2)
                .HasColumnName("planned_value");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ProjectedFinishDate).HasColumnName("projected_finish_date");
            entity.Property(e => e.ProjectedTotalCost)
                .HasPrecision(15, 2)
                .HasColumnName("projected_total_cost");
            entity.Property(e => e.Spi)
                .HasPrecision(15, 2)
                .HasColumnName("spi");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMetric)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_metric_project_id_fkey");
        });

        modelBuilder.Entity<ProjectPosition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_position_pkey");

            entity.ToTable("project_position");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("assigned_at");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");
            entity.Property(e => e.ProjectMemberId).HasColumnName("project_member_id");

            entity.HasOne(d => d.ProjectMember).WithMany(p => p.ProjectPosition)
                .HasForeignKey(d => d.ProjectMemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_position_project_member_id_fkey");
        });

        modelBuilder.Entity<ProjectRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_recommendation_pkey");

            entity.ToTable("project_recommendation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Recommendation).HasColumnName("recommendation");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectRecommendation)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_recommendation_project_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.ProjectRecommendation)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_recommendation_task_id_fkey");
        });

        modelBuilder.Entity<RecipientNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recipient_notification_pkey");

            entity.ToTable("recipient_notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Account).WithMany(p => p.RecipientNotification)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recipient_notification_account_id_fkey");

            entity.HasOne(d => d.Notification).WithMany(p => p.RecipientNotification)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recipient_notification_notification_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("refresh_token_pkey");

            entity.ToTable("refresh_token");

            entity.Property(e => e.RefreshTokenId).HasColumnName("refresh_token_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");

            entity.HasOne(d => d.Account).WithMany(p => p.RefreshToken)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("fk_refresh_token_account");
        });

        modelBuilder.Entity<Requirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("requirement_pkey");

            entity.ToTable("requirement");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .HasColumnName("priority");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.Requirement)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("requirement_project_id_fkey");
        });

        modelBuilder.Entity<Risk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("risk_pkey");

            entity.ToTable("risk");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.GeneratedBy)
                .HasMaxLength(100)
                .HasColumnName("generated_by");
            entity.Property(e => e.ImpactLevel)
                .HasMaxLength(50)
                .HasColumnName("impact_level");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.Probability)
                .HasMaxLength(50)
                .HasColumnName("probability");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ResponsibleId).HasColumnName("responsible_id");
            entity.Property(e => e.RiskScope)
                .HasMaxLength(255)
                .HasColumnName("risk_scope");
            entity.Property(e => e.SeverityLevel)
                .HasMaxLength(50)
                .HasColumnName("severity_level");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.Risk)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_project_id_fkey");

            entity.HasOne(d => d.Responsible).WithMany(p => p.Risk)
                .HasForeignKey(d => d.ResponsibleId)
                .HasConstraintName("risk_responsible_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Risk)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("risk_task_id_fkey");
        });

        modelBuilder.Entity<RiskSolution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("risk_solution_pkey");

            entity.ToTable("risk_solution");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContingencyPlan).HasColumnName("contingency_plan");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MitigationPlan).HasColumnName("mitigation_plan");
            entity.Property(e => e.RiskId).HasColumnName("risk_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Risk).WithMany(p => p.RiskSolution)
                .HasForeignKey(d => d.RiskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_solution_risk_id_fkey");
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sprint_pkey");

            entity.ToTable("sprint");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Goal).HasColumnName("goal");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Project).WithMany(p => p.Sprint)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sprint_project_id_fkey");
        });

        modelBuilder.Entity<Subtask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subtask_pkey");

            entity.ToTable("subtask");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.GenerationAiInput)
                .HasDefaultValue(false)
                .HasColumnName("generation_ai_input");
            entity.Property(e => e.ManualInput)
                .HasDefaultValue(false)
                .HasColumnName("manual_input");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .HasColumnName("priority");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.SprintId).HasColumnName("sprint_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.SubtaskAssignedByNavigation)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("subtask_assigned_by_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.SubtaskReporter)
                .HasForeignKey(d => d.ReporterId)
                .HasConstraintName("subtask_reporter_id_fkey");

            entity.HasOne(d => d.Sprint).WithMany(p => p.Subtask)
                .HasForeignKey(d => d.SprintId)
                .HasConstraintName("subtask_sprint_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Subtask)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtask_task_id_fkey");
        });

        modelBuilder.Entity<SubtaskComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subtask_comment_pkey");

            entity.ToTable("subtask_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");

            entity.HasOne(d => d.Account).WithMany(p => p.SubtaskComment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtask_comment_account_id_fkey");

            entity.HasOne(d => d.Subtask).WithMany(p => p.SubtaskComment)
                .HasForeignKey(d => d.SubtaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtask_comment_subtask_id_fkey");
        });

        modelBuilder.Entity<SubtaskFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subtask_file_pkey");

            entity.ToTable("subtask_file");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UrlFile)
                .HasMaxLength(1024)
                .HasColumnName("url_file");

            entity.HasOne(d => d.Subtask).WithMany(p => p.SubtaskFile)
                .HasForeignKey(d => d.SubtaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtask_file_subtask_id_fkey");
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("system_configuration_pkey");

            entity.ToTable("system_configuration");

            entity.HasIndex(e => e.ConfigKey, "system_configuration_config_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConfigKey)
                .HasMaxLength(255)
                .HasColumnName("config_key");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EffectedFrom).HasColumnName("effected_from");
            entity.Property(e => e.EffectedTo).HasColumnName("effected_to");
            entity.Property(e => e.EstimateValue)
                .HasMaxLength(255)
                .HasColumnName("estimate_value");
            entity.Property(e => e.MaxValue)
                .HasMaxLength(255)
                .HasColumnName("max_value");
            entity.Property(e => e.MinValue)
                .HasMaxLength(255)
                .HasColumnName("min_value");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValueConfig).HasColumnName("value_config");
        });

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_assignment_pkey");

            entity.ToTable("task_assignment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.ActualHours)
                .HasPrecision(8, 2)
                .HasColumnName("actual_hours");
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.PlannedHours)
                .HasPrecision(8, 2)
                .HasColumnName("planned_hours");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");

            entity.HasOne(d => d.Account).WithMany(p => p.TaskAssignment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_assignment_account_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskAssignment)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_assignment_task_id_fkey");
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_comment_pkey");

            entity.ToTable("task_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");

            entity.HasOne(d => d.Account).WithMany(p => p.TaskComment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_comment_account_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskComment)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_comment_task_id_fkey");
        });

        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_dependency_pkey");

            entity.ToTable("task_dependency");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LinkedFrom)
                .HasMaxLength(255)
                .HasColumnName("linked_from");
            entity.Property(e => e.LinkedTo)
                .HasMaxLength(255)
                .HasColumnName("linked_to");
            entity.Property(e => e.MilestoneId).HasColumnName("milestone_id");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.LinkedFromNavigation).WithMany(p => p.TaskDependencyLinkedFromNavigation)
                .HasForeignKey(d => d.LinkedFrom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_dependency_linked_from_fkey");

            entity.HasOne(d => d.LinkedToNavigation).WithMany(p => p.TaskDependencyLinkedToNavigation)
                .HasForeignKey(d => d.LinkedTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_dependency_linked_to_fkey");

            entity.HasOne(d => d.Milestone).WithMany(p => p.TaskDependency)
                .HasForeignKey(d => d.MilestoneId)
                .HasConstraintName("task_dependency_milestone_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskDependencyTask)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("task_dependency_task_id_fkey");
        });

        modelBuilder.Entity<TaskFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_file_pkey");

            entity.ToTable("task_file");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UrlFile)
                .HasMaxLength(1024)
                .HasColumnName("url_file");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskFile)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_file_task_id_fkey");
        });

        modelBuilder.Entity<Tasks>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tasks_pkey");

            entity.ToTable("tasks");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.ActualCost)
                .HasPrecision(15, 2)
                .HasColumnName("actual_cost");
            entity.Property(e => e.ActualEndDate).HasColumnName("actual_end_date");
            entity.Property(e => e.ActualHours)
                .HasPrecision(8, 2)
                .HasColumnName("actual_hours");
            entity.Property(e => e.ActualResourceCost)
                .HasPrecision(15, 2)
                .HasColumnName("actual_resource_cost");
            entity.Property(e => e.ActualStartDate).HasColumnName("actual_start_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration)
                .HasMaxLength(100)
                .HasColumnName("duration");
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");
            entity.Property(e => e.Evaluate)
                .HasMaxLength(50)
                .HasColumnName("evaluate");
            entity.Property(e => e.GenerationAiInput)
                .HasDefaultValue(false)
                .HasColumnName("generation_ai_input");
            entity.Property(e => e.ManualInput)
                .HasDefaultValue(false)
                .HasColumnName("manual_input");
            entity.Property(e => e.PercentComplete)
                .HasPrecision(5, 2)
                .HasColumnName("percent_complete");
            entity.Property(e => e.PlannedCost)
                .HasPrecision(15, 2)
                .HasColumnName("planned_cost");
            entity.Property(e => e.PlannedEndDate).HasColumnName("planned_end_date");
            entity.Property(e => e.PlannedHours)
                .HasPrecision(8, 2)
                .HasColumnName("planned_hours");
            entity.Property(e => e.PlannedResourceCost)
                .HasPrecision(15, 2)
                .HasColumnName("planned_resource_cost");
            entity.Property(e => e.PlannedStartDate).HasColumnName("planned_start_date");
            entity.Property(e => e.Priority)
                .HasMaxLength(50)
                .HasColumnName("priority");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.RemainingHours)
                .HasPrecision(8, 2)
                .HasColumnName("remaining_hours");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.SprintId).HasColumnName("sprint_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Epic).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.EpicId)
                .HasConstraintName("tasks_epic_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_project_id_fkey");

            entity.HasOne(d => d.Reporter).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_reporter_id_fkey");

            entity.HasOne(d => d.Sprint).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.SprintId)
                .HasConstraintName("tasks_sprint_id_fkey");
        });

        modelBuilder.Entity<WorkItemLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("work_item_label_pkey");

            entity.ToTable("work_item_label");

            entity.HasIndex(e => new { e.LabelId, e.TaskId, e.EpicId, e.SubtaskId }, "work_item_label_label_id_task_id_epic_id_subtask_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.LabelId).HasColumnName("label_id");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");

            entity.HasOne(d => d.Epic).WithMany(p => p.WorkItemLabel)
                .HasForeignKey(d => d.EpicId)
                .HasConstraintName("work_item_label_epic_id_fkey");

            entity.HasOne(d => d.Label).WithMany(p => p.WorkItemLabel)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("work_item_label_label_id_fkey");

            entity.HasOne(d => d.Subtask).WithMany(p => p.WorkItemLabel)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("work_item_label_subtask_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.WorkItemLabel)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("work_item_label_task_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
