using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("RefreshToken");

        entity.HasKey(e => e.TokenId).HasName("RefreshToken_pkey");

        entity.Property(e => e.TokenId)
            .HasColumnName("tokenId")
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(e => e.UserId)
            .HasColumnName("userId");

        entity.Property(e => e.TokenHash)
            .HasColumnName("tokenHash")
            .HasColumnType("text");

        entity.Property(e => e.ExpiresAt)
            .HasColumnName("expiresAt")
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.CreatedAt)
            .HasColumnName("createdAt")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()");

        entity.Property(e => e.RevokedAt)
            .HasColumnName("revokedAt")
            .HasColumnType("timestamp with time zone");

        entity.Property(e => e.ReplacedByToken)
            .HasColumnName("replacedByToken");

        entity.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("RefreshToken_userId_fkey");

        entity.HasIndex(e => e.TokenHash)
            .HasDatabaseName("RefreshToken_tokenHash_idx");
    }
}
