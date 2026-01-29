using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.HasKey(e => e.CategoryId).HasName("Category_pkey");

        entity.ToTable("Category");

        entity.HasIndex(e => e.Name, "Category_name_key").IsUnique();

        entity.Property(e => e.CategoryId).HasColumnName("categoryId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character varying");
        entity.Property(e => e.Description).HasColumnName("description");
        entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.CreatedBy).HasColumnName("createdBy");

        entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Categories)
            .HasForeignKey(d => d.CreatedBy)
            .HasConstraintName("Category_createdBy_fkey");
    }
}
