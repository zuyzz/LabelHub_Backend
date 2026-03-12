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
        entity.Property(e => e.AnnotationId).HasColumnName("annotationId");
        entity.Property(e => e.TaskId).HasColumnName("taskId");
        entity.Property(e => e.ReviewerId).HasColumnName("reviewerId");
        entity.Property(e => e.Result)
            .HasColumnName("result")
            .HasColumnType("character varying")
            .HasConversion<string>();
        entity.Property(e => e.Feedback).HasColumnName("feedback");
        entity.Property(e => e.ReviewedAt).HasColumnName("reviewedAt");

        entity.HasOne(d => d.ReviewAnnotation).WithMany(p => p.Reviews)
            .HasForeignKey(d => d.AnnotationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_annotationId_fkey");

        entity.HasOne(d => d.ReviewTask).WithMany(p => p.Reviews)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_taskId_fkey");

        entity.HasOne(d => d.ReviewUser).WithMany(p => p.ReviewUsers)
            .HasForeignKey(d => d.ReviewerId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Review_reviewerId_fkey");
    }
}