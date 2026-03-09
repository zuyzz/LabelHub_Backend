using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class ProjectMember
{
    public Guid ProjectId { get; set; }

    public DateTime JoinedAt { get; set; }

    public Guid MemberId { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User ProjectMemberUser { get; set; } = null!;
}
