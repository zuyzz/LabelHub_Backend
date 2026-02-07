using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.HasKey(e => e.ProjectId).HasName("Project_pkey");

        entity.ToTable("Project");

        entity.Property(e => e.ProjectId).HasColumnName("projectId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.Status).HasColumnName("status")
            .HasDefaultValueSql("'active'::character varying")
            .HasColumnType("character varying");
        entity.Property(e => e.CategoryId).HasColumnName("categoryId");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");

        entity.HasOne(d => d.ProjectCategory).WithMany(p => p.Projects)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Project_categoryId_fkey");

        entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Projects)
            .HasForeignKey(d => d.CreatedBy)
            .HasConstraintName("Project_createdBy_fkey");

        entity.HasMany(p => p.ProjectVersions)
            .WithOne(pv => pv.Project)
            .HasForeignKey(pv => pv.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ProjectVersion_projectId_fkey");
    }
}
