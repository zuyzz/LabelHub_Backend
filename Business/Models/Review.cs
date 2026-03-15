using System;
using System.Collections.Generic;
using DataLabelProject.Business.Models.Enums;

namespace DataLabelProject.Business.Models;

public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid TaskItemId { get; set; }

    public Guid ReviewerId { get; set; }

    public ReviewResult Result { get; set; }

    public string? Feedback { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual LabelingTaskItem ReviewTaskItem { get; set; } = null!;

    public virtual User ReviewUser { get; set; } = null!;
}
