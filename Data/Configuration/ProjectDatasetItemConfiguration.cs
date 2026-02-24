using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectDatasetItemConfiguration : IEntityTypeConfiguration<ProjectDatasetItem>
{
    public void Configure(EntityTypeBuilder<ProjectDatasetItem> entity)
    {
        entity.HasKey(e => new { e.ProjectId, e.DatasetItemId }).HasName("ProjectDatasetItem_pkey");

        entity.ToTable("ProjectDatasetItem");

        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.DatasetItemId).HasColumnName("datasetItemId");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.DatasetItem).WithMany(p => p.ProjectDatasetItems)
            .HasForeignKey(d => d.DatasetItemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectDatasetItem_datasetItemId_fkey");

        entity.HasOne(d => d.Project).WithMany(p => p.ProjectDatasetItems)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectDatasetItem_projectId_fkey");
    }
}
