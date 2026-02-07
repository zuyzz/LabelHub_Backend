using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Project
{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Dataset> Datasets { get; set; } = new List<Dataset>();

    public virtual ICollection<ExportJob> ExportJobs { get; set; } = new List<ExportJob>();

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    
    public ICollection<ProjectVersion> ProjectVersions { get; set; } = new List<ProjectVersion>();

    public virtual Category ProjectCategory { get; set; } = null!;

    public virtual User? CreatedByUser { get; set; }
}
