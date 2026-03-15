using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> entity)
    {
        entity.HasKey(e => e.ReviewId).HasName("Review_pkey");

        entity.ToTable("Review");

        entity.Property(e => e.ReviewId).HasColumnName("reviewId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.TaskItemId).HasColumnName("taskItemId");
        entity.Property(e => e.ReviewerId).HasColumnName("reviewerId");
        entity.Property(e => e.Result)
            .HasColumnName("result")
            .HasColumnType("enum_review_result");
        entity.Property(e => e.Feedback).HasColumnName("feedback");
        entity.Property(e => e.ReviewedAt).HasColumnName("reviewedAt");

        entity.HasOne(d => d.ReviewTaskItem).WithOne(p => p.Review)
            .HasForeignKey<Review>(d => d.TaskItemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_taskItemId_fkey");

        entity.HasOne(d => d.ReviewUser).WithMany(p => p.Reviews)
            .HasForeignKey(d => d.ReviewerId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_reviewerId_fkey");
    }
}