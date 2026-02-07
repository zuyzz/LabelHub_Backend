using System;
using System.Collections.Generic;

namespace DataLabel_Project_BE.Models
{
    public class ProjectVersion
    {
        public Guid ProjectVersionId { get; set; }

        public Guid ProjectId { get; set; }
        public Guid DatasetId { get; set; }
        public Guid LabelSetId { get; set; }
        public Guid GuidelineId { get; set; }

        public int VersionNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        public Project Project { get; set; } = null!;
        public Dataset Dataset { get; set; } = null!;
        public LabelSet LabelSet { get; set; } = null!;
        public Guideline Guideline { get; set; } = null!;
    }
}
