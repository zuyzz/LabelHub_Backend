using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ConsensusConfiguration : IEntityTypeConfiguration<Consensus>
{
    public void Configure(EntityTypeBuilder<Consensus> entity)
    {
        entity.HasKey(e => e.ConsensusId).HasName("Consensus_pkey");

        entity.ToTable("Consensus");

        entity.Property(e => e.ConsensusId).HasColumnName("consensusId").HasDefaultValueSql("gen_random_uuid()");
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");
        entity.Property(e => e.DatasetItemId).HasColumnName("datasetItemId");
        entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb");

        entity.HasOne(d => d.DatasetItem).WithMany()
            .HasForeignKey(d => d.DatasetItemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Consensus_datasetItemId_fkey");
    }
}
