using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class GuidelineConfiguration : IEntityTypeConfiguration<Guideline>
{
    public void Configure(EntityTypeBuilder<Guideline> entity)
    {
        entity.HasKey(e => e.GuidelineId).HasName("Guideline_pkey");

        entity.ToTable("Guideline");

        entity.Property(e => e.GuidelineId).HasColumnName("guidelineId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Title).HasColumnName("title").HasColumnType("character varying");
        entity.Property(e => e.Content).HasColumnName("content");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");

        entity.HasOne(d => d.GuidelineProject).WithMany(p => p.Guidelines)
            .HasForeignKey(d => d.ProjectId)
            .HasConstraintName("Guideline_projectId_fkey");
    }
}
