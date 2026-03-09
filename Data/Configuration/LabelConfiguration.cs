using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> entity)
    {
        entity.HasKey(e => e.LabelId).HasName("Label_pkey");

        entity.ToTable("Label");

        entity.Property(e => e.LabelId).HasColumnName("labelId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.CategoryId).HasColumnName("categoryId");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");

        entity.HasOne(d => d.LabelCategory).WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Label_categoryId_fkey");

        entity.HasOne(d => d.LabelCreatedByUser).WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Label_createdBy_fkey");
    }
}
