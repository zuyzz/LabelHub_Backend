using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class ProjectTemplate
{
    public Guid TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public MediaType MediaType { get; set; } = MediaType.Image;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
