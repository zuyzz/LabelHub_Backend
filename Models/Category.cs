using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models;

public partial class Category
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual User? CreatedByUser { get; set; }
}
