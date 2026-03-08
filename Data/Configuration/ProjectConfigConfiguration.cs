using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectConfigConfiguration : IEntityTypeConfiguration<ProjectConfig>
{
    public void Configure(EntityTypeBuilder<ProjectConfig> entity)
    {
        entity.HasKey(e => e.ProjectConfigId).HasName("ProjectConfig_pkey");

        entity.ToTable("ProjectConfig");

        entity.Property(e => e.ProjectConfigId).HasColumnName("projectConfigId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.DefaultAnnotateDeadlineInterval).HasColumnName("defaultAnnotateDeadlineInterval");
        entity.Property(e => e.DefaultReviewDeadlineInterval).HasColumnName("defaultReviewDeadlineInterval");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.AgreementThreshold).HasColumnName("agreementThreshold");
        entity.Property(e => e.MinimumAnnotationsPerTask).HasColumnName("minimumAnnotationsPerTask").HasDefaultValue(3);

        entity.HasOne(d => d.Project).WithMany(p => p.ProjectConfigs)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectConfig_projectId_fkey");
    }
}
