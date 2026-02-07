using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration
{
    public class ProjectVersionConfiguration : IEntityTypeConfiguration<ProjectVersion>
    {
        public void Configure(EntityTypeBuilder<ProjectVersion> builder)
        {
            builder.ToTable("ProjectVersion");

            // Primary key
            builder.HasKey(pv => pv.ProjectVersionId);

            builder.Property(pv => pv.ProjectVersionId)
                   .HasColumnName("projectVersionId")
                   .HasDefaultValueSql("gen_random_uuid()");

            // Required foreign keys
            builder.Property(pv => pv.ProjectId)
                   .HasColumnName("projectId")
                   .IsRequired();

            builder.Property(pv => pv.DatasetId)
                   .HasColumnName("datasetId")
                   .IsRequired();

            builder.Property(pv => pv.LabelSetId)
                   .HasColumnName("labelSetId")
                   .IsRequired();

            builder.Property(pv => pv.GuidelineId)
                   .HasColumnName("guidelineId")
                   .IsRequired();

            // Version number
            builder.Property(pv => pv.VersionNumber)
                   .HasColumnName("versionNumber")
                   .IsRequired();

            // Timestamps
            builder.Property(pv => pv.CreatedAt)
                   .HasColumnName("createdAt")
                   .HasDefaultValueSql("NOW()")
                   .IsRequired();

            builder.Property(pv => pv.ReleasedAt)
                   .HasColumnName("releasedAt");

            // Unique constraint (projectId, versionNumber)
            builder.HasIndex(pv => new { pv.ProjectId, pv.VersionNumber })
                   .IsUnique()
                   .HasDatabaseName("uq_ProjectVersion_project_version");

            // Relationships
            builder.HasOne(pv => pv.Project)
                   .WithMany(p => p.ProjectVersions)
                   .HasForeignKey(pv => pv.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pv => pv.Dataset)
                   .WithMany()
                   .HasForeignKey(pv => pv.DatasetId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pv => pv.LabelSet)
                   .WithMany()
                   .HasForeignKey(pv => pv.LabelSetId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pv => pv.Guideline)
                   .WithMany()
                   .HasForeignKey(pv => pv.GuidelineId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
