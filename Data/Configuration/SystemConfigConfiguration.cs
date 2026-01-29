using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> entity)
    {
        entity.HasKey(e => e.SystemConfigId).HasName("SystemConfig_pkey");

        entity.ToTable("SystemConfig");

        entity.Property(e => e.SystemConfigId).HasColumnName("systemConfigId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.AnnotateDeadlineConfig).HasColumnName("annotateDeadlineConfig");
        entity.Property(e => e.ReviewDeadlineInterval).HasColumnName("reviewDeadlineInterval");
    }
}
