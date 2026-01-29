using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabel_Project_BE.Data.Configuration;

public class AnnotationConfiguration : IEntityTypeConfiguration<Annotation>
{
    public void Configure(EntityTypeBuilder<Annotation> entity)
    {
        entity.HasKey(e => e.AnnotationId).HasName("Annotation_pkey");

        entity.ToTable("Annotation");

        entity.Property(e => e.AnnotationId).HasColumnName("annotationId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.TaskId).HasColumnName("taskId");
        entity.Property(e => e.AnnotatorId).HasColumnName("annotatorId");
        entity.Property(e => e.LabelSetId).HasColumnName("labelSetId");
        entity.Property(e => e.LabelSetVersionNumber).HasColumnName("labelSetVersionNumber");
        entity.Property(e => e.AnnotationPayload).HasColumnName("annotationPayload").HasColumnType("jsonb");
        entity.Property(e => e.IsDraft).HasColumnName("isDraft");
        entity.Property(e => e.SubmittedAt).HasColumnName("submittedAt");

        entity.HasOne(d => d.AnnotationAnnotator).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.AnnotatorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_annotatorId_fkey");

        entity.HasOne(d => d.AnnotationLabelSet).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.LabelSetId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_labelSetId_fkey");

        entity.HasOne(d => d.AnnotationTask).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_taskId_fkey");
    }
}
