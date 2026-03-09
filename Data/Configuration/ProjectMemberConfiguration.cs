using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> entity)
    {
        entity.HasKey(e => new { e.ProjectId, e.MemberId }).HasName("ProjectMember_pkey");

        entity.ToTable("ProjectMember");

        entity.Property(e => e.ProjectId).HasColumnName("projectId");
        entity.Property(e => e.MemberId).HasColumnName("memberId");
        entity.Property(e => e.JoinedAt).HasColumnName("joinedAt").HasDefaultValueSql("now()");

        entity.HasOne(d => d.Project).WithMany(p => p.ProjectMembers)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ProjectMember_projectId_fkey");

        entity.HasOne(d => d.ProjectMemberUser).WithMany(p => p.ProjectMembers)
            .HasForeignKey(d => d.MemberId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("ProjectMember_memberId_fkey");
    }
}
