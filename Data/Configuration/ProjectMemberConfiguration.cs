using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> entity)
    {
        entity.HasKey(e => e.ProjectMemberId).HasName("ProjectMember_pkey");

        entity.ToTable("ProjectMember");

        entity.Property(e => e.ProjectMemberId).HasColumnName("projectMemberId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.UserId).HasColumnName("userId");
        entity.Property(e => e.JoinedAt).HasColumnName("joinedAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.Project).WithMany(p => p.ProjectMembers)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ProjectMember_projectId_fkey");

        entity.HasOne(d => d.ProjectMemberUser).WithMany(p => p.ProjectMembers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ProjectMember_userId_fkey");
    }
}