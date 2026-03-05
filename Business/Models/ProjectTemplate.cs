using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class ProjectTemplate
{
    public Guid TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string MediaType { get; set; } = "image";

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
