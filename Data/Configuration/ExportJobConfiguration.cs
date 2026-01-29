using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class ExportJobConfiguration : IEntityTypeConfiguration<ExportJob>
{
    public void Configure(EntityTypeBuilder<ExportJob> entity)
    {
        entity.HasKey(e => e.ExportId).HasName("ExportJob_pkey");

        entity.ToTable("ExportJob");

        entity.Property(e => e.ExportId).HasColumnName("exportId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.InitiatorId).HasColumnName("initiatorId");
        entity.Property(e => e.TargetProjectId).HasColumnName("targetProjectId");
        entity.Property(e => e.Format).HasColumnName("format").HasColumnType("character varying");
        entity.Property(e => e.Status).HasColumnName("status").HasColumnType("character varying");
        entity.Property(e => e.FileUri).HasColumnName("fileUri");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.ExportInitiator).WithMany(p => p.ExportJobs)
            .HasForeignKey(d => d.InitiatorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("ExportJob_initiatorId_fkey");

        entity.HasOne(d => d.TargetProject).WithMany(p => p.ExportJobs)
            .HasForeignKey(d => d.TargetProjectId)
            .HasConstraintName("ExportJob_targetProjectId_fkey");
    }
}
