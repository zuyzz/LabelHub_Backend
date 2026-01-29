using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
{
    public void Configure(EntityTypeBuilder<Dataset> entity)
    {
        entity.HasKey(e => e.DatasetId).HasName("Dataset_pkey");

        entity.ToTable("Dataset");

        entity.Property(e => e.DatasetId).HasColumnName("datasetId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.StorageUri).HasColumnName("storageUri");
        entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        entity.Property(e => e.VersionNumber).HasColumnName("versionNumber").HasDefaultValue(1);
        entity.Property(e => e.CurrentLabelSetId).HasColumnName("currentLabelSetId");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");

        entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Datasets)
            .HasForeignKey(d => d.CreatedBy)
            .HasConstraintName("Dataset_createdBy_fkey");

        entity.HasOne(d => d.CurrentLabelSet).WithMany(p => p.Datasets)
            .HasForeignKey(d => d.CurrentLabelSetId)
            .HasConstraintName("Dataset_currentLabelSetId_fkey");

        entity.HasOne(d => d.DatasetProject).WithMany(p => p.Datasets)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Dataset_projectId_fkey");
    }
}
