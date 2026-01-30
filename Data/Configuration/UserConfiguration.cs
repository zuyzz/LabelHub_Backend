using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.UserId).HasName("Users_pkey");

        entity.HasIndex(e => e.Email, "Users_email_key").IsUnique();

        entity.HasIndex(e => e.PhoneNumber, "Users_phoneNumber_key").IsUnique();

        entity.HasIndex(e => e.Username, "Users_username_key").IsUnique();

        entity.Property(e => e.UserId).HasColumnName("userId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.Username).HasColumnName("username").HasColumnType("character varying");
        entity.Property(e => e.PasswordHash).HasColumnName("passwordHash");
        entity.Property(e => e.DisplayName).HasColumnName("displayName").HasColumnType("character varying");
        entity.Property(e => e.Email).HasColumnName("email").HasColumnType("character varying");
        entity.Property(e => e.PhoneNumber).HasColumnName("phoneNumber")
            .HasComment("VN format: 0xxxxxxxxx or +84xxxxxxxxx")
            .HasColumnType("character varying");
        entity.Property(e => e.RoleId).HasColumnName("roleId");
        entity.Property(e => e.IsActive).HasColumnName("isActive").HasDefaultValue(true);
        entity.Property(e => e.CreatedAt).HasColumnName("createdAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.UserRole).WithMany(p => p.Users)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Users_roleId_fkey");
    }
}
