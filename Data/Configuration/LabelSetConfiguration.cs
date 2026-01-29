using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class LabelSetConfiguration : IEntityTypeConfiguration<LabelSet>
{
    public void Configure(EntityTypeBuilder<LabelSet> entity)
    {
        entity.HasKey(e => e.LabelSetId).HasName("LabelSet_pkey");

        entity.ToTable("LabelSet");

        entity.Property(e => e.LabelSetId).HasColumnName("labelSetId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.VersionNumber).HasColumnName("versionNumber").HasDefaultValue(1);
        entity.Property(e => e.GuidelineId).HasColumnName("guidelineId");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");

        entity.HasOne(d => d.CreatedByUser).WithMany(p => p.LabelSets)
            .HasForeignKey(d => d.CreatedBy)
            .HasConstraintName("LabelSet_createdBy_fkey");

        entity.HasOne(d => d.LabelSetGuideline).WithMany(p => p.LabelSets)
            .HasForeignKey(d => d.GuidelineId)
            .HasConstraintName("LabelSet_guidelineId_fkey");
    }
}
