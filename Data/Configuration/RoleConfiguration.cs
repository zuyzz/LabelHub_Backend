using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.HasKey(e => e.RoleId).HasName("Role_pkey");

        entity.ToTable("Role");

        entity.HasIndex(e => e.RoleName, "Role_roleName_key").IsUnique();

        entity.Property(e => e.RoleId).HasColumnName("roleId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.RoleName).HasColumnName("roleName").HasColumnType("character varying");
    }
}
