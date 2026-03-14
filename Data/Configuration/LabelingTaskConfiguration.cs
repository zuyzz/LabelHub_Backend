using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class LabelingTaskConfiguration : IEntityTypeConfiguration<LabelingTask>
{
    public void Configure(EntityTypeBuilder<LabelingTask> entity)
    {
        entity.HasKey(e => e.TaskId).HasName("LabelingTask_pkey");

        entity.ToTable("LabelingTask");

        entity.Property(e => e.TaskId).HasColumnName("taskId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.DatasetItemId).HasColumnName("datasetItemId");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");

        entity.HasOne(d => d.LabelingTaskDatasetItem).WithMany(p => p.LabelingTasks)
            .HasForeignKey(d => d.DatasetItemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("LabelingTask_datasetItemId_fkey");

        entity.HasOne(d => d.LabelingTaskProject).WithMany(p => p.LabelingTasks)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("LabelingTask_projectId_fkey");
    }
}
