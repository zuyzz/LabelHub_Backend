using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    /// <summary>
    /// VN format: 0xxxxxxxxx or +84xxxxxxxxx
    /// </summary>
    public string? PhoneNumber { get; set; }

    public Guid RoleId { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates if user must change password on first login
    /// </summary>
    public bool IsFirstLogin { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

    public virtual ICollection<Assignment> AssignmentAssignedByUsers { get; set; } = new List<Assignment>();

    public virtual ICollection<Assignment> AssignmentUsers { get; set; } = new List<Assignment>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();

    public virtual ICollection<ExportJob> ExportJobs { get; set; } = new List<ExportJob>();

    public virtual ICollection<LabelSet> LabelSets { get; set; } = new List<LabelSet>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    public virtual ICollection<Review> ReviewApprovedByUsers { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewUsers { get; set; } = new List<Review>();

    public virtual Role UserRole { get; set; } = null!;
}
