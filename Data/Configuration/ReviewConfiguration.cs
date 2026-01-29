using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> entity)
    {
        entity.HasKey(e => e.ReviewId).HasName("Review_pkey");

        entity.ToTable("Review");

        entity.Property(e => e.ReviewId).HasColumnName("reviewId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.AnnotationId).HasColumnName("annotationId");
        entity.Property(e => e.ReviewerId).HasColumnName("reviewerId");
        entity.Property(e => e.Result).HasColumnName("result").HasColumnType("character varying");
        entity.Property(e => e.Feedback).HasColumnName("feedback");
        entity.Property(e => e.DeadlineAt).HasColumnName("deadlineAt");
        entity.Property(e => e.IsApproved).HasColumnName("isApproved");
        entity.Property(e => e.ApprovedBy).HasColumnName("approvedBy");
        entity.Property(e => e.ApprovedAt).HasColumnName("approvedAt");
        entity.Property(e => e.ReviewedAt).HasColumnName("reviewedAt");

        entity.HasOne(d => d.ReviewAnnotation).WithMany(p => p.Reviews)
            .HasForeignKey(d => d.AnnotationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_annotationId_fkey");

        entity.HasOne(d => d.ApprovedByUser).WithMany(p => p.ReviewApprovedByUsers)
            .HasForeignKey(d => d.ApprovedBy)
            .HasConstraintName("Review_approvedBy_fkey");

        entity.HasOne(d => d.ReviewUser).WithMany(p => p.ReviewUsers)
            .HasForeignKey(d => d.ReviewerId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_reviewerId_fkey");
    }
}
