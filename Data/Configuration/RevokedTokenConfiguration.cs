using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> entity)
    {
        entity.HasKey(e => e.Jti).HasName("RevokedToken_pkey");
        entity.ToTable("RevokedToken");

        entity.Property(e => e.Jti).HasColumnName("jti").HasColumnType("character varying(128)");
        entity.Property(e => e.ExpiresAt).HasColumnName("expiresAt");
        entity.Property(e => e.RevokedAt).HasColumnName("revokedAt").HasDefaultValueSql("now()");
    }
}
