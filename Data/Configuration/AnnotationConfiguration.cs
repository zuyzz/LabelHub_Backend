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
        entity.Property(e => e.TaskItemId).HasColumnName("taskItemId");
        entity.Property(e => e.AnnotatorId).HasColumnName("annotatorId");
        entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb");
        entity.Property(e => e.SubmittedAt).HasColumnName("submittedAt");
        entity.Property(e => e.Note).HasColumnName("note");

        entity.HasOne(d => d.AnnotationAnnotator).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.AnnotatorId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_annotatorId_fkey");

        entity.HasOne(d => d.AnnotationTaskItem).WithMany(p => p.Annotations)
            .HasForeignKey(d => d.TaskItemId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("Annotation_taskItemId_fkey");
    }
}
