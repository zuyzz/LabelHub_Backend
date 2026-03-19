using System.Text.Json;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Consensus;

namespace DataLabelProject.Business.Services.Shared;

/// <summary>
/// Helper service to extract and convert bounding boxes from annotation payloads.
/// Centralizes box extraction logic used by both Annotation and Consensus services.
/// </summary>
public class BoxConversionHelper
{
    /// <summary>
    /// Flattens and validates bounding boxes from annotation payloads.
    /// </summary>
    public static List<BoxCandidate> FlattenBoxes(IEnumerable<Annotation> annotations)
    {
        var boxes = new List<BoxCandidate>();

        foreach (var annotation in annotations)
        {
            if (string.IsNullOrWhiteSpace(annotation.Payload))
                continue;

            AnnotationPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<AnnotationPayload>(annotation.Payload);
            }
            catch
            {
                continue;
            }

            if (payload?.Bboxes == null)
                continue;

            foreach (var box in payload.Bboxes)
            {
                // Validate box has all required properties
                if (box.X == null || box.Y == null || box.Width == null || box.Height == null)
                    continue;

                if (string.IsNullOrWhiteSpace(box.Label) || box.Width <= 0 || box.Height <= 0)
                    continue;

                boxes.Add(new BoxCandidate
                {
                    AnnotatorId = annotation.AnnotatorId,
                    Label = box.Label.Trim(),
                    X = box.X.Value,
                    Y = box.Y.Value,
                    Width = box.Width.Value,
                    Height = box.Height.Value
                });
            }
        }

        return boxes;
    }
}
