using System;
using System.Collections.Generic;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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

    public virtual DbSet<Aggregatedcounter> Aggregatedcounter { get; set; }

    public virtual DbSet<AiResponseEvaluation> AiResponseEvaluation { get; set; }

    public virtual DbSet<AiResponseHistory> AiResponseHistory { get; set; }

    public virtual DbSet<ChangeRequest> ChangeRequest { get; set; }

    public virtual DbSet<Counter> Counter { get; set; }

    public virtual DbSet<Document> Document { get; set; }

    public virtual DbSet<DocumentComment> DocumentComment { get; set; }

    public virtual DbSet<DocumentExportFile> DocumentExportFile { get; set; }

    public virtual DbSet<DocumentPermission> DocumentPermission { get; set; }

    public virtual DbSet<DynamicCategory> DynamicCategory { get; set; }

    public virtual DbSet<Epic> Epic { get; set; }

    public virtual DbSet<EpicComment> EpicComment { get; set; }

    public virtual DbSet<EpicFile> EpicFile { get; set; }

    public virtual DbSet<Hash> Hash { get; set; }

    public virtual DbSet<Job> Job { get; set; }

    public virtual DbSet<Jobparameter> Jobparameter { get; set; }

    public virtual DbSet<Jobqueue> Jobqueue { get; set; }

    public virtual DbSet<Label> Label { get; set; }

    public virtual DbSet<List> List { get; set; }

    public virtual DbSet<Lock> Lock { get; set; }

    public virtual DbSet<Meeting> Meeting { get; set; }

    public virtual DbSet<MeetingDocument> MeetingDocument { get; set; }

    public virtual DbSet<MeetingLog> MeetingLog { get; set; }

    public virtual DbSet<MeetingParticipant> MeetingParticipant { get; set; }

    public virtual DbSet<MeetingRescheduleRequest> MeetingRescheduleRequest { get; set; }

    public virtual DbSet<MeetingSummary> MeetingSummary { get; set; }

    public virtual DbSet<MeetingTranscript> MeetingTranscript { get; set; }

    public virtual DbSet<Milestone> Milestone { get; set; }

    public virtual DbSet<MilestoneComment> MilestoneComment { get; set; }

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

    public virtual DbSet<RiskComment> RiskComment { get; set; }

    public virtual DbSet<RiskFile> RiskFile { get; set; }

    public virtual DbSet<RiskSolution> RiskSolution { get; set; }

    public virtual DbSet<Schema> Schema { get; set; }

    public virtual DbSet<Server> Server { get; set; }

    public virtual DbSet<Set> Set { get; set; }

    public virtual DbSet<Sprint> Sprint { get; set; }

    public virtual DbSet<State> State { get; set; }

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

    public virtual DbSet<WorkLog> WorkLog { get; set; }

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

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql("Host=shuttle.proxy.rlwy.net;Port=46730;Database=SU25_SEP490_IntelliPM;Username=postgres;Password=ePBNfZQAuyaFhaDvPboiVTGaPikaSUrP;");

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
            entity.Property(e => e.EpicId)
                .HasMaxLength(255)
                .HasColumnName("epic_id");
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
            entity.Property(e => e.RiskKey)
                .HasMaxLength(255)
                .HasColumnName("risk_key");
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

            entity.HasOne(d => d.Epic).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.EpicId)
                .HasConstraintName("activity_log_epic_id_fkey");

            entity.HasOne(d => d.RiskKeyNavigation).WithMany(p => p.ActivityLog)
                .HasPrincipalKey(p => p.RiskKey)
                .HasForeignKey(d => d.RiskKey)
                .HasConstraintName("fk_activity_log_risk");

            entity.HasOne(d => d.Subtask).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("activity_log_subtask_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.ActivityLog)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("activity_log_task_id_fkey");
        });

        modelBuilder.Entity<Aggregatedcounter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("aggregatedcounter_pkey");

            entity.ToTable("aggregatedcounter", "hangfire");

            entity.HasIndex(e => e.Key, "aggregatedcounter_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<AiResponseEvaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_response_evaluation_pkey");

            entity.ToTable("ai_response_evaluation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AiResponseId).HasColumnName("ai_response_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Account).WithMany(p => p.AiResponseEvaluation)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_response_evaluation_account_id_fkey");

            entity.HasOne(d => d.AiResponse).WithMany(p => p.AiResponseEvaluation)
                .HasForeignKey(d => d.AiResponseId)
                .HasConstraintName("ai_response_evaluation_ai_response_id_fkey");
        });

        modelBuilder.Entity<AiResponseHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_response_history_pkey");

            entity.ToTable("ai_response_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AiFeature)
                .HasMaxLength(100)
                .HasColumnName("ai_feature");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ResponseJson)
                .HasColumnType("jsonb")
                .HasColumnName("response_json");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AiResponseHistory)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_response_history_created_by_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.AiResponseHistory)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ai_response_history_project_id_fkey");
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

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counter_pkey");

            entity.ToTable("counter", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_counter_expireat");

            entity.HasIndex(e => e.Key, "ix_hangfire_counter_key");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
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
            entity.Property(e => e.Visibility)
                .HasMaxLength(20)
                .HasColumnName("visibility");

            entity.HasOne(d => d.Approver).WithMany(p => p.DocumentApprover)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("fk_document_approver_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.DocumentCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_created_by");

            entity.HasOne(d => d.Epic).WithMany(p => p.Document)
                .HasForeignKey(d => d.EpicId)
                .HasConstraintName("fk_document_epic");

            entity.HasOne(d => d.Project).WithMany(p => p.Document)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_document_project");

            entity.HasOne(d => d.Subtask).WithMany(p => p.Document)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("fk_document_subtask");

            entity.HasOne(d => d.Task).WithMany(p => p.Document)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("fk_document_task");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.DocumentUpdatedByNavigation)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("fk_document_updated_by");
        });

        modelBuilder.Entity<DocumentComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_comment_pkey");

            entity.ToTable("document_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Author).WithMany(p => p.DocumentComment)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("fk_author");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentComment)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("fk_document");
        });

        modelBuilder.Entity<DocumentExportFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_export_file_pkey");

            entity.ToTable("document_export_file");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.ExportedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("exported_at");
            entity.Property(e => e.ExportedBy).HasColumnName("exported_by");
            entity.Property(e => e.ExportedFileUrl)
                .HasMaxLength(1000)
                .HasColumnName("exported_file_url");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentExportFile)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("fk_document_export");

            entity.HasOne(d => d.ExportedByNavigation).WithMany(p => p.DocumentExportFile)
                .HasForeignKey(d => d.ExportedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_exported_by");
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
                .HasMaxLength(50)
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

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hash_pkey");

            entity.ToTable("hash", "hangfire");

            entity.HasIndex(e => new { e.Key, e.Field }, "hash_key_field_key").IsUnique();

            entity.HasIndex(e => e.Expireat, "ix_hangfire_hash_expireat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Field).HasColumnName("field");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_pkey");

            entity.ToTable("job", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_job_expireat");

            entity.HasIndex(e => e.Statename, "ix_hangfire_job_statename");

            entity.HasIndex(e => e.Statename, "ix_hangfire_job_statename_is_not_null").HasFilter("(statename IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Arguments)
                .HasColumnType("jsonb")
                .HasColumnName("arguments");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Invocationdata)
                .HasColumnType("jsonb")
                .HasColumnName("invocationdata");
            entity.Property(e => e.Stateid).HasColumnName("stateid");
            entity.Property(e => e.Statename).HasColumnName("statename");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
        });

        modelBuilder.Entity<Jobparameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobparameter_pkey");

            entity.ToTable("jobparameter", "hangfire");

            entity.HasIndex(e => new { e.Jobid, e.Name }, "ix_hangfire_jobparameter_jobidandname");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Job).WithMany(p => p.Jobparameter)
                .HasForeignKey(d => d.Jobid)
                .HasConstraintName("jobparameter_jobid_fkey");
        });

        modelBuilder.Entity<Jobqueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobqueue_pkey");

            entity.ToTable("jobqueue", "hangfire");

            entity.HasIndex(e => new { e.Fetchedat, e.Queue, e.Jobid }, "ix_hangfire_jobqueue_fetchedat_queue_jobid").HasNullSortOrder(new[] { NullSortOrder.NullsFirst, NullSortOrder.NullsLast, NullSortOrder.NullsLast });

            entity.HasIndex(e => new { e.Jobid, e.Queue }, "ix_hangfire_jobqueue_jobidandqueue");

            entity.HasIndex(e => new { e.Queue, e.Fetchedat }, "ix_hangfire_jobqueue_queueandfetchedat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fetchedat).HasColumnName("fetchedat");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
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

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("list_pkey");

            entity.ToTable("list", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_list_expireat");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Lock>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("lock", "hangfire");

            entity.HasIndex(e => e.Resource, "lock_resource_key").IsUnique();

            entity.Property(e => e.Acquired).HasColumnName("acquired");
            entity.Property(e => e.Resource).HasColumnName("resource");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
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

        modelBuilder.Entity<MilestoneComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("milestone_comment_pkey");

            entity.ToTable("milestone_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.MilestoneId).HasColumnName("milestone_id");

            entity.HasOne(d => d.Account).WithMany(p => p.MilestoneComment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestone_comment_account_id_fkey");

            entity.HasOne(d => d.Milestone).WithMany(p => p.MilestoneComment)
                .HasForeignKey(d => d.MilestoneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestone_comment_milestone_id_fkey");
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
            entity.Property(e => e.WorkingHoursPerDay)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("8")
                .HasColumnName("working_hours_per_day");

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
            entity.Property(e => e.BudgetAtCompletion)
                .HasPrecision(15, 2)
                .HasColumnName("budget_at_completion");
            entity.Property(e => e.CalculatedBy)
                .HasMaxLength(50)
                .HasColumnName("calculated_by");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(5, 2)
                .HasColumnName("confidence_score");
            entity.Property(e => e.CostPerformanceIndex)
                .HasPrecision(15, 2)
                .HasColumnName("cost_performance_index");
            entity.Property(e => e.CostVariance)
                .HasPrecision(15, 2)
                .HasColumnName("cost_variance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationAtCompletion)
                .HasPrecision(15, 2)
                .HasColumnName("duration_at_completion");
            entity.Property(e => e.EarnedValue)
                .HasPrecision(15, 2)
                .HasColumnName("earned_value");
            entity.Property(e => e.EstimateAtCompletion)
                .HasPrecision(15, 2)
                .HasColumnName("estimate_at_completion");
            entity.Property(e => e.EstimateDurationAtCompletion)
                .HasPrecision(15, 2)
                .HasColumnName("estimate_duration_at_completion");
            entity.Property(e => e.EstimateToComplete)
                .HasPrecision(15, 2)
                .HasColumnName("estimate_to_complete");
            entity.Property(e => e.ImprovementSummary)
                .HasDefaultValueSql("''::text")
                .HasColumnName("improvement_summary");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.IsImproved)
                .HasDefaultValue(false)
                .HasColumnName("is_improved");
            entity.Property(e => e.PlannedValue)
                .HasPrecision(15, 2)
                .HasColumnName("planned_value");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.SchedulePerformanceIndex)
                .HasPrecision(15, 2)
                .HasColumnName("schedule_performance_index");
            entity.Property(e => e.ScheduleVariance)
                .HasPrecision(15, 2)
                .HasColumnName("schedule_variance");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.VarianceAtCompletion)
                .HasPrecision(15, 2)
                .HasColumnName("variance_at_completion");

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
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Recommendation).HasColumnName("recommendation");
            entity.Property(e => e.SuggestedChanges).HasColumnName("suggested_changes");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectRecommendation)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_recommendation_project_id_fkey");
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

            entity.HasIndex(e => e.RiskKey, "risk_risk_key_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
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
            entity.Property(e => e.RiskKey)
                .HasMaxLength(20)
                .HasColumnName("risk_key");
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

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.RiskCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_created_by_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Risk)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_project_id_fkey");

            entity.HasOne(d => d.Responsible).WithMany(p => p.RiskResponsible)
                .HasForeignKey(d => d.ResponsibleId)
                .HasConstraintName("risk_responsible_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Risk)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("risk_task_id_fkey");
        });

        modelBuilder.Entity<RiskComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("risk_comment_pkey");

            entity.ToTable("risk_comment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.RiskId).HasColumnName("risk_id");

            entity.HasOne(d => d.Account).WithMany(p => p.RiskComment)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_comment_account_id_fkey");

            entity.HasOne(d => d.Risk).WithMany(p => p.RiskComment)
                .HasForeignKey(d => d.RiskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_comment_risk_id_fkey");
        });

        modelBuilder.Entity<RiskFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("risk_file_pkey");

            entity.ToTable("risk_file");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(1024)
                .HasColumnName("file_url");
            entity.Property(e => e.RiskId).HasColumnName("risk_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");

            entity.HasOne(d => d.Risk).WithMany(p => p.RiskFile)
                .HasForeignKey(d => d.RiskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_file_risk_id_fkey");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.RiskFile)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("risk_file_uploaded_by_fkey");
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

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("schema_pkey");

            entity.ToTable("schema", "hangfire");

            entity.Property(e => e.Version)
                .ValueGeneratedNever()
                .HasColumnName("version");
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("server_pkey");

            entity.ToTable("server", "hangfire");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Lastheartbeat).HasColumnName("lastheartbeat");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("set_pkey");

            entity.ToTable("set", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_set_expireat");

            entity.HasIndex(e => new { e.Key, e.Score }, "ix_hangfire_set_key_score");

            entity.HasIndex(e => new { e.Key, e.Value }, "set_key_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat).HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
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
            entity.Property(e => e.PlannedEndDate).HasColumnName("planned_end_date");
            entity.Property(e => e.PlannedStartDate).HasColumnName("planned_start_date");
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

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("state_pkey");

            entity.ToTable("state", "hangfire");

            entity.HasIndex(e => e.Jobid, "ix_hangfire_state_jobid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat).HasColumnName("createdat");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");

            entity.HasOne(d => d.Job).WithMany(p => p.State)
                .HasForeignKey(d => d.Jobid)
                .HasConstraintName("state_jobid_fkey");
        });

        modelBuilder.Entity<Subtask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subtask_pkey");

            entity.ToTable("subtask");

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
            entity.Property(e => e.AssignedBy).HasColumnName("assigned_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration)
                .HasMaxLength(100)
                .HasColumnName("duration");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
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
            entity.Property(e => e.RemainingHours)
                .HasPrecision(8, 2)
                .HasColumnName("remaining_hours");
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
            entity.Property(e => e.FromType)
                .HasMaxLength(50)
                .HasColumnName("from_type");
            entity.Property(e => e.LinkedFrom)
                .HasMaxLength(255)
                .HasColumnName("linked_from");
            entity.Property(e => e.LinkedTo)
                .HasMaxLength(255)
                .HasColumnName("linked_to");
            entity.Property(e => e.ToType)
                .HasMaxLength(50)
                .HasColumnName("to_type");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
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

        modelBuilder.Entity<WorkLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("work_log_pkey");

            entity.ToTable("work_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Hours)
                .HasPrecision(8, 2)
                .HasColumnName("hours");
            entity.Property(e => e.LogDate).HasColumnName("log_date");
            entity.Property(e => e.SubtaskId)
                .HasMaxLength(255)
                .HasColumnName("subtask_id");
            entity.Property(e => e.TaskId)
                .HasMaxLength(255)
                .HasColumnName("task_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Subtask).WithMany(p => p.WorkLog)
                .HasForeignKey(d => d.SubtaskId)
                .HasConstraintName("work_log_subtask_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.WorkLog)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("work_log_task_id_fkey");
        });
        modelBuilder.HasSequence("course_id_seq");
        modelBuilder.HasSequence("flower_id_seq");
        modelBuilder.HasSequence("intellipm_id_seq");
        modelBuilder.HasSequence("projc_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
