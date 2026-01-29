using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> entity)
    {
        entity.HasKey(e => e.ActivityLogId).HasName("ActivityLog_pkey");

        entity.ToTable("ActivityLog");

        entity.Property(e => e.ActivityLogId).HasColumnName("activityLogId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.UserId).HasColumnName("userId");
        entity.Property(e => e.EventType).HasColumnName("eventType").HasColumnType("character varying");
        entity.Property(e => e.TargetEntity).HasColumnName("targetEntity").HasColumnType("character varying");
        entity.Property(e => e.TargetId).HasColumnName("targetId");
        entity.Property(e => e.Details).HasColumnName("details").HasColumnType("jsonb");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.Project).WithMany(p => p.ActivityLogs)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ActivityLog_projectId_fkey");

        entity.HasOne(d => d.ActivityLogUser).WithMany(p => p.ActivityLogs)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("ActivityLog_userId_fkey");
    }
}
