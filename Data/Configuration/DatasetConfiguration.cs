using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
{
    public void Configure(EntityTypeBuilder<Dataset> entity)
    {
        entity.HasKey(e => e.DatasetId).HasName("Dataset_pkey");

        entity.ToTable("Dataset");

        entity.Property(e => e.DatasetId).HasColumnName("datasetId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
        entity.Property(e => e.MediaType).HasColumnName("mediaType").HasColumnType("character varying").HasDefaultValue("image");

        entity.HasOne(d => d.CreatedByUser).WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .HasConstraintName("Dataset_createdBy_fkey");
    }
}
