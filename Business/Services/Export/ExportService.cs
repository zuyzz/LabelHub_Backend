using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataLabelProject.Data;

namespace DataLabelProject.Business.Services.Export;

public class ExportService : IExportService
{
    private readonly AppDbContext _appDbContext;

    public ExportService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Stream> ExportAsCocoJsonAsync(Guid datasetId)
    {
        // Load dataset items with all related data
        var items = await _appDbContext.DatasetItems
            .Where(i => i.DatasetId == datasetId)
            .ToListAsync();

        if (!items.Any())
            throw new InvalidOperationException("Dataset has no items");

        // Load all annotation tasks for these items
        var taskIds = await _appDbContext.AnnotationTasks
            .Where(t => t.DatasetId == datasetId)
            .Select(t => t.TaskId)
            .ToListAsync();

        var annotations = await _appDbContext.Annotations
            .Where(a => taskIds.Contains(a.TaskId) && !a.IsDraft && a.SubmittedAt.HasValue)
            .Include(a => a.AnnotationTask)
            .ToListAsync();

        // Get all labels used
        var labelSetIds = annotations.Select(a => a.LabelSetId).Distinct().ToList();
        var labels = await _appDbContext.Labels
            .Where(l => labelSetIds.Contains(l.LabelSetId))
            .ToListAsync();

        // Build COCO structure
        var cocoImages = new List<object>();
        var cocoAnnotations = new List<object>();
        var cocoCategories = new Dictionary<string, object>(); // category name -> category object

        int imageId = 1;
        int annotationId = 1;
        int categoryId = 1;

        // Process each item
        foreach (var item in items)
        {
            // Parse metadata to get dimensions
            var (width, height) = ParseMetadata(item.Metadata);

            // Add to COCO images
            cocoImages.Add(new
            {
                id = imageId,
                file_name = Path.GetFileName(item.StorageUri),
                width = width,
                height = height
            });

            // Get annotations for this item
            var itemAnnotations = annotations
                .Where(a => a.AnnotationTask.ScopeUri == item.StorageUri)
                .ToList();

            // Process each annotation
            foreach (var annotation in itemAnnotations)
            {
                try
                {
                    var payload = ParseAnnotationPayload(annotation.AnnotationPayload);
                    
                    foreach (var shape in payload)
                    {
                        var labelName = shape.Label ?? "unknown";

                        // Add category if not exists
                        if (!cocoCategories.ContainsKey(labelName))
                        {
                            cocoCategories[labelName] = new
                            {
                                id = categoryId,
                                name = labelName,
                                supercategory = "object"
                            };
                            categoryId++;
                        }

                        var catId = ((dynamic)cocoCategories[labelName]).id;

                        // Add annotation based on shape type
                        var annotationObj = CreateCocoAnnotation(
                            annotationId++,
                            imageId,
                            catId,
                            shape,
                            width,
                            height);

                        cocoAnnotations.Add(annotationObj);
                    }
                }
                catch (Exception ex)
                {
                    // Log and skip malformed annotations
                    System.Diagnostics.Debug.WriteLine($"Error parsing annotation {annotation.AnnotationId}: {ex.Message}");
                }
            }

            imageId++;
        }

        // Build final COCO document
        var cocoDoc = new
        {
            info = new
            {
                description = "COCO dataset exported from LabelHub",
                version = "1.0",
                year = DateTime.Now.Year,
                date_created = DateTime.UtcNow.ToString("u")
            },
            licenses = new List<object>(),
            images = cocoImages,
            annotations = cocoAnnotations,
            categories = cocoCategories.Values.OrderBy(c => ((dynamic)c).id).ToList()
        };

        // Serialize to JSON
        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, leaveOpen: true))
        {
            var json = JsonSerializer.Serialize(cocoDoc, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            });
            await writer.WriteAsync(json);
            await writer.FlushAsync();
        }

        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Parse annotation payload JSON.
    /// Expected format: array of objects with "label" and optional "bbox" or "points"
    /// </summary>
    private List<AnnotationShape> ParseAnnotationPayload(string payload)
    {
        var shapes = new List<AnnotationShape>();

        if (string.IsNullOrWhiteSpace(payload))
            return shapes;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            // Handle array of annotations
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    var shape = new AnnotationShape();

                    // Extract label
                    if (item.TryGetProperty("label", out var labelProp))
                        shape.Label = labelProp.GetString();

                    // Extract bbox [x, y, w, h]
                    if (item.TryGetProperty("bbox", out var bboxProp) && bboxProp.ValueKind == JsonValueKind.Array)
                    {
                        var bboxArray = bboxProp.EnumerateArray().Select(v => v.GetDouble()).ToArray();
                        if (bboxArray.Length >= 4)
                            shape.BBox = new[] { bboxArray[0], bboxArray[1], bboxArray[2], bboxArray[3] };
                    }

                    // Extract points [[x1,y1], [x2,y2], ...]
                    if (item.TryGetProperty("points", out var pointsProp) && pointsProp.ValueKind == JsonValueKind.Array)
                    {
                        var points = new List<double>();
                        foreach (var point in pointsProp.EnumerateArray())
                        {
                            if (point.ValueKind == JsonValueKind.Array)
                            {
                                var coords = point.EnumerateArray().Select(v => v.GetDouble()).ToArray();
                                points.AddRange(coords);
                            }
                        }
                        if (points.Any())
                            shape.Points = points.ToArray();
                    }

                    if (!string.IsNullOrEmpty(shape.Label))
                        shapes.Add(shape);
                }
            }
            // Handle single object
            else if (root.ValueKind == JsonValueKind.Object)
            {
                var shape = new AnnotationShape();

                if (root.TryGetProperty("label", out var labelProp))
                    shape.Label = labelProp.GetString();

                if (root.TryGetProperty("bbox", out var bboxProp) && bboxProp.ValueKind == JsonValueKind.Array)
                {
                    var bboxArray = bboxProp.EnumerateArray().Select(v => v.GetDouble()).ToArray();
                    if (bboxArray.Length >= 4)
                        shape.BBox = new[] { bboxArray[0], bboxArray[1], bboxArray[2], bboxArray[3] };
                }

                if (root.TryGetProperty("points", out var pointsProp) && pointsProp.ValueKind == JsonValueKind.Array)
                {
                    var points = new List<double>();
                    foreach (var point in pointsProp.EnumerateArray())
                    {
                        if (point.ValueKind == JsonValueKind.Array)
                        {
                            var coords = point.EnumerateArray().Select(v => v.GetDouble()).ToArray();
                            points.AddRange(coords);
                        }
                    }
                    if (points.Any())
                        shape.Points = points.ToArray();
                }

                if (!string.IsNullOrEmpty(shape.Label))
                    shapes.Add(shape);
            }
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse annotation payload: {ex.Message}");
        }

        return shapes;
    }

    /// <summary>
    /// Create a COCO annotation object from a shape
    /// </summary>
    private object CreateCocoAnnotation(int annotationId, int imageId, int categoryId, AnnotationShape shape, int imgWidth, int imgHeight)
    {
        double[] bbox = shape.BBox ?? new[] { 0.0, 0.0, 0.0, 0.0 };
        double area = bbox[2] * bbox[3];

        var annotObj = new
        {
            id = annotationId,
            image_id = imageId,
            category_id = categoryId,
            bbox = bbox,
            area = area,
            segmentation = shape.Points?.Length > 0 ? new List<double[]> { shape.Points } : new List<double[]>(),
            iscrowd = 0
        };

        return annotObj;
    }

    /// <summary>
    /// Parse metadata JSON to extract width and height
    /// </summary>
    private (int width, int height) ParseMetadata(string? metadata)
    {
        // Default dimensions
        int width = 1024;
        int height = 768;

        if (string.IsNullOrWhiteSpace(metadata))
            return (width, height);

        try
        {
            using var doc = JsonDocument.Parse(metadata);
            var root = doc.RootElement;

            if (root.TryGetProperty("width", out var widthProp) && widthProp.TryGetInt32(out var w))
                width = w;

            if (root.TryGetProperty("height", out var heightProp) && heightProp.TryGetInt32(out var h))
                height = h;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse metadata: {ex.Message}");
        }

        return (width, height);
    }

    /// <summary>
    /// Internal class representing an annotation shape
    /// </summary>
    private class AnnotationShape
    {
        public string? Label { get; set; }
        public double[]? BBox { get; set; }  // [x, y, w, h]
        public double[]? Points { get; set; } // [x1, y1, x2, y2, ...]
    }
}
