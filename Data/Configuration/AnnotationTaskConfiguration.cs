using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class AnnotationTaskConfiguration : IEntityTypeConfiguration<AnnotationTask>
{
    public void Configure(EntityTypeBuilder<AnnotationTask> entity)
    {
        entity.HasKey(e => e.TaskId).HasName("AnnotationTask_pkey");

        entity.ToTable("AnnotationTask");

        entity.Property(e => e.TaskId).HasColumnName("taskId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.DatasetId).HasColumnName("datasetId");
        entity.Property(e => e.ScopeUri).HasColumnName("scopeUri");
        entity.Property(e => e.Status).HasColumnName("status").HasColumnType("character varying");
        entity.Property(e => e.Consensus).HasColumnName("consensus").HasColumnType("jsonb");
        entity.Property(e => e.DeadlineAt).HasColumnName("deadlineAt");
        entity.Property(e => e.AssignedAt).HasColumnName("assignedAt");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.TaskDataset).WithMany(p => p.AnnotationTasks)
            .HasForeignKey(d => d.DatasetId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("AnnotationTask_datasetId_fkey");
    }
}
