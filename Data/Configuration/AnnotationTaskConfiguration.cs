using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class AnnotationTaskConfiguration : IEntityTypeConfiguration<AnnotationTask>
{
    public void Configure(EntityTypeBuilder<AnnotationTask> entity)
    {
        entity.HasKey(e => e.TaskId).HasName("Task_pkey");

        entity.ToTable("Task");

        entity.Property(e => e.TaskId).HasColumnName("taskId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.DatasetItemId).HasColumnName("datasetItemId");
        entity.Property(e => e.ScopeUri).HasColumnName("scopeUri");
        entity.Property(e => e.Status).HasColumnName("status").HasColumnType("character varying");
        entity.Property(e => e.Consensus).HasColumnName("consensus").HasColumnType("jsonb");
        entity.Property(e => e.DeadlineAt).HasColumnName("deadlineAt");
        entity.Property(e => e.AssignedAt).HasColumnName("assignedAt");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.Deleted).HasColumnName("deleted").HasDefaultValue(false);

        entity.HasOne(d => d.TaskDatasetItem).WithMany(p => p.AnnotationTasks)
            .HasForeignKey(d => d.DatasetItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("Task_datasetItemId_fkey");
    }
}
