using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectLabelConfiguration : IEntityTypeConfiguration<ProjectLabel>
{
    public void Configure(EntityTypeBuilder<ProjectLabel> entity)
    {
        entity.HasKey(e => new { e.ProjectId, e.LabelId }).HasName("ProjectLabel_pkey");

        entity.ToTable("ProjectLabel");

        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.LabelId).HasColumnName("labelId");
        entity.Property(e => e.AttachedAt).HasColumnName("attachedAt").HasDefaultValueSql("now()");
        entity.Property(e => e.AttachedBy).HasColumnName("attachedBy");

        entity.HasOne(d => d.Project).WithMany(p => p.ProjectLabels)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectLabel_projectId_fkey");

        entity.HasOne(d => d.Label).WithMany()
            .HasForeignKey(d => d.LabelId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ProjectLabel_labelId_fkey");

        entity.HasOne(d => d.AttachedByUser).WithMany()
            .HasForeignKey(d => d.AttachedBy)
            .HasConstraintName("ProjectLabel_attachedBy_fkey");
    }
}
