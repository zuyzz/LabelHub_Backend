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
        entity.Property(e => e.LabelSetId).HasColumnName("labelSetId");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);

        entity.HasOne(d => d.LabelLabelSet).WithMany(p => p.Labels)
            .HasForeignKey(d => d.LabelSetId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Label_labelSetId_fkey");

        entity.HasIndex(e => new { e.LabelSetId, e.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Label_LabelSetId_Name");
    }
}
