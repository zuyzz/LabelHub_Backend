using DataLabelProject.Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLabelProject.Data.Configuration;

public class AnnotationConfiguration : IEntityTypeConfiguration<Annotation>
{
    public void Configure(EntityTypeBuilder<Annotation> entity)
    {
        entity.HasKey(e => e.AnnotationId).HasName("Annotation_pkey");

        entity.ToTable("Annotation");

        entity.Property(e => e.AnnotationId).HasColumnName("annotationId").HasDefaultValueSql("uuid_generate_v4()");
        entity.Property(e => e.TaskId).HasColumnName("taskId");
        entity.Property(e => e.AnnotatorId).HasColumnName("annotatorId");
        entity.Ignore(e => e.LabelSetId);
        entity.Ignore(e => e.LabelSetVersionNumber);
        entity.Property(e => e.AnnotationPayload).HasColumnName("annotationPayload").HasColumnType("jsonb");
        entity.Property(e => e.IsDraft).HasColumnName("isDraft");
        entity.Property(e => e.SubmittedAt).HasColumnName("submittedAt");

        entity.HasOne(d => d.AnnotationAnnotator).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.AnnotatorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_annotatorId_fkey");

        entity.Ignore(e => e.AnnotationLabelSet);

        entity.HasOne(d => d.AnnotationTask).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.TaskId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_taskId_fkey");
    }
}
