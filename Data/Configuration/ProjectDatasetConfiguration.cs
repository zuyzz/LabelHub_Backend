using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectDatasetConfiguration : IEntityTypeConfiguration<ProjectDataset>
{
    public void Configure(EntityTypeBuilder<ProjectDataset> entity)
    {
        entity.HasKey(e => new { e.ProjectId, e.DatasetId }).HasName("ProjectDataset_pkey");

        entity.ToTable("ProjectDataset");

        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.DatasetId).HasColumnName("datasetId");
        entity.Property(e => e.AttachedAt).HasColumnName("attachedAt").HasDefaultValueSql("now()");
        entity.Property(e => e.AttachedBy).HasColumnName("attachedBy");

        entity.HasOne(d => d.Project)
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .HasConstraintName("ProjectDataset_projectId_fkey");

        entity.HasOne(d => d.Dataset)
            .WithMany(d => d.ProjectDatasets)
            .HasForeignKey(d => d.DatasetId)
            .HasConstraintName("ProjectDataset_datasetId_fkey");

        entity.HasOne(d => d.AttachedByUser)
            .WithMany()
            .HasForeignKey(d => d.AttachedBy)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectDataset_attachedBy_fkey");
    }
}
