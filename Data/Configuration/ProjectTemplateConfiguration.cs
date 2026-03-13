using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectTemplateConfiguration : IEntityTypeConfiguration<ProjectTemplate>
{
    public void Configure(EntityTypeBuilder<ProjectTemplate> entity)
    {
        entity.HasKey(e => e.TemplateId).HasName("ProjectTemplate_pkey");

        entity.ToTable("ProjectTemplate");

        entity.Property(e => e.TemplateId).HasColumnName("templateId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("ProjectTemplate_name_key");
        entity.Property(e => e.MediaType)
            .HasColumnName("mediaType")
            .HasColumnType("enum_media_type")
            .HasConversion<string>()
            .HasDefaultValue(MediaType.Image);
    }
}
