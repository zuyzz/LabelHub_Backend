using System;
using System.Collections.Generic;

namespace DataLabelProject.Business.Models;

public partial class ProjectMember
{
    public Guid ProjectMemberId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User ProjectMemberUser { get; set; } = null!;
}
