using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> entity)
    {
        entity.HasKey(e => e.AssignmentId).HasName("Assignment_pkey");

        entity.ToTable("Assignment");

        entity.Property(e => e.AssignmentId).HasColumnName("assignmentId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.TaskId).HasColumnName("taskId");
        entity.Property(e => e.AssignedTo).HasColumnName("assignedTo");
        entity.Property(e => e.AssignedBy).HasColumnName("assignedBy");
        entity.Property(e => e.AssignedAt).HasColumnName("assignedAt").HasColumnType("timestamp with time zone").HasDefaultValueSql("now()");
        entity.Property(e => e.StartedAt).HasColumnName("startedAt").HasColumnType("timestamp with time zone");
        entity.Property(e => e.TimeLimitMinutes).HasColumnName("timeLimitMinutes");

        entity.HasOne(d => d.AssignedByUser).WithMany(p => p.Assignments)
            .HasForeignKey(d => d.AssignedBy)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Assignment_assignedBy_fkey");

        entity.HasOne(d => d.AssignmentTask).WithMany(p => p.Assignments)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Assignment_taskId_fkey");

        entity.HasOne(d => d.AssignmentUser).WithMany(p => p.AssignmentUsers)
            .HasForeignKey(d => d.AssignedTo)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Assignment_assignedTo_fkey");
    }
}
