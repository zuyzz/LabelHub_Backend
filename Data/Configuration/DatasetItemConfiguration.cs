using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataLabelProject.Business.Models;

namespace DataLabelProject.Data.Configuration;

public class DatasetItemConfiguration : IEntityTypeConfiguration<DatasetItem>
{
    public void Configure(EntityTypeBuilder<DatasetItem> entity)
    {
        entity.ToTable("DatasetItem");
        
        entity.HasKey(e => e.ItemId);
        
        entity.Property(e => e.ItemId)
            .HasColumnName("itemId");
        
        entity.Property(e => e.DatasetId)
            .HasColumnName("datasetId")
            .IsRequired();
        
        entity.Property(e => e.MediaType)
            .HasColumnName("mediaType")
            .HasMaxLength(50)
            .IsRequired();
        
        entity.Property(e => e.StorageUri)
            .HasColumnName("storageUri")
            .HasMaxLength(500)
            .IsRequired();
        
        entity.Property(e => e.CreatedAt)
            .HasColumnName("createdAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Relationship: DatasetItem belongs to Dataset
        entity.HasOne(d => d.Dataset)
            .WithMany(p => p.DatasetItems)
            .HasForeignKey(d => d.DatasetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("DatasetItem_datasetId_fkey");
    }
}
