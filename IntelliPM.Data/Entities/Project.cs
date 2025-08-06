using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class Project
{
    public int Id { get; set; }

    public string ProjectKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Budget { get; set; }

    public string ProjectType { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? IconUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ChangeRequest> ChangeRequest { get; set; } = new List<ChangeRequest>();

    public virtual Account CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual ICollection<Epic> Epic { get; set; } = new List<Epic>();

    public virtual ICollection<Label> Label { get; set; } = new List<Label>();

    public virtual ICollection<Meeting> Meeting { get; set; } = new List<Meeting>();

    public virtual ICollection<Milestone> Milestone { get; set; } = new List<Milestone>();

    public virtual ICollection<ProjectMember> ProjectMember { get; set; } = new List<ProjectMember>();

    public virtual ICollection<ProjectMetric> ProjectMetric { get; set; } = new List<ProjectMetric>();

    public virtual ICollection<ProjectRecommendation> ProjectRecommendation { get; set; } = new List<ProjectRecommendation>();

    public virtual ICollection<Requirement> Requirement { get; set; } = new List<Requirement>();

    public virtual ICollection<Risk> Risk { get; set; } = new List<Risk>();

    public virtual ICollection<Sprint> Sprint { get; set; } = new List<Sprint>();

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}
