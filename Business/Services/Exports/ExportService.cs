using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataLabelProject.Application.DTOs.Exports;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Models.Enums;
using DataLabelProject.Business.Services.Storage;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Business.Services.Exports;

public class ExportService : IExportService
{
    private readonly IExportJobRepository _exportJobRepository;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorage _fileStorage;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExportService(
        IExportJobRepository exportJobRepository,
        AppDbContext context,
        ICurrentUserService currentUserService,
        IFileStorage fileStorage)
    {
        _exportJobRepository = exportJobRepository;
        _context = context;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
    }

    public async Task<IEnumerable<ExportJobResponse>> GetExports()
    {
        var exports = await _exportJobRepository.GetAllAsync();
        return exports.Select(MapToResponse);
    }

    public async Task<ExportJobResponse?> GetExportById(Guid exportId)
    {
        var export = await _exportJobRepository.GetByIdAsync(exportId);
        return export == null ? null : MapToResponse(export);
    }

    public async Task<ExportJobResponse> CreateExport(Guid projectId, CreateExportRequest request)
    {
        var format = request.Format.ToLowerInvariant();
        if (format != "json" && format != "coco" && format != "yolo")
            throw new ArgumentException("Format must be one of: json, coco, yolo");

        var project = await _context.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project == null)
            throw new InvalidOperationException("Project not found");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User not authenticated");

        // Generate a new export ID for file paths
        var exportId = Guid.NewGuid();

        try
        {
            // Build the unified dataset from consensus annotations
            var dataset = await BuildDatasetFromConsensus(projectId);

            // Generate file based on format
            string fileUri;
            switch (format)
            {
                case "coco":
                    fileUri = await GenerateCoco(dataset, exportId);
                    break;
                case "yolo":
                    fileUri = await GenerateYolo(dataset, exportId);
                    break;
                default: // json
                    fileUri = await GenerateJson(dataset, exportId);
                    break;
            }

            // Create export job record with completed status
            var exportJob = new ExportJob
            {
                ExportId = exportId,
                InitiatorId = userId,
                ProjectId = projectId,
                Format = format,
                Status = ExportJobStatus.completed,
                FileUri = fileUri
            };

            await _exportJobRepository.CreateAsync(exportJob);
            await _exportJobRepository.SaveChangesAsync();

            return MapToResponse(exportJob);
        }
        catch (Exception)
        {
            // Create export job record with failed status
            var exportJob = new ExportJob
            {
                ExportId = exportId,
                InitiatorId = userId,
                ProjectId = projectId,
                Format = format,
                Status = ExportJobStatus.failed
            };

            await _exportJobRepository.CreateAsync(exportJob);
            await _exportJobRepository.SaveChangesAsync();

            throw;
        }
    }

    // ─── Dataset Building ─────────────────────────────────────────────

    private async Task<ExportDataset> BuildDatasetFromConsensus(Guid projectId)
    {
        // Get all labeling tasks for this project, with their dataset items and consensus
        var tasks = await _context.LabelingTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.LabelingTaskDatasetItem)
            .ToListAsync();

        var taskIds = tasks.Select(t => t.TaskId).ToList();

        // Get all consensuses for these tasks
        var consensuses = await _context.Consensuses
            .AsNoTracking()
            .Where(c => taskIds.Contains(c.TaskId))
            .ToListAsync();

        var consensusByTaskId = consensuses.ToDictionary(c => c.TaskId);

        var dataset = new ExportDataset();
        var categoriesMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int imageIdCounter = 1;
        int annotationIdCounter = 1;

        foreach (var task in tasks)
        {
            // Skip tasks without consensus
            if (!consensusByTaskId.TryGetValue(task.TaskId, out var consensus))
                continue;

            // Parse consensus payload
            var objects = ParseConsensusPayload(consensus.Payload);
            if (objects == null || objects.Count == 0)
                continue;

            var datasetItem = task.LabelingTaskDatasetItem;
            var imageId = imageIdCounter++;

            // Parse metadata for image dimensions
            var (width, height) = ParseImageDimensions(datasetItem.Metadata);

            var fileName = ExtractFileName(datasetItem.StorageUri);

            dataset.Images.Add(new ExportImage
            {
                Id = imageId,
                FileName = fileName,
                Width = width,
                Height = height
            });

            foreach (var obj in objects)
            {
                // Get or create category
                if (!categoriesMap.TryGetValue(obj.Label, out var categoryId))
                {
                    categoryId = categoriesMap.Count + 1;
                    categoriesMap[obj.Label] = categoryId;
                    dataset.Categories.Add(new ExportCategory
                    {
                        Id = categoryId,
                        Name = obj.Label
                    });
                }

                dataset.Annotations.Add(new ExportAnnotation
                {
                    Id = annotationIdCounter++,
                    ImageId = imageId,
                    CategoryId = categoryId,
                    Bbox = new double[] { obj.X, obj.Y, obj.W, obj.H }
                });
            }
        }

        return dataset;
    }

    // ─── Format Generators ────────────────────────────────────────────

    private async Task<string> GenerateJson(ExportDataset dataset, Guid exportId)
    {
        var json = JsonSerializer.Serialize(dataset, JsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using var stream = new MemoryStream(bytes);
        var path = $"exports/{exportId}/dataset.json";
        return await _fileStorage.CreateFileAsync(stream, path, "application/json");
    }

    private async Task<string> GenerateCoco(ExportDataset dataset, Guid exportId)
    {
        var coco = new
        {
            images = dataset.Images.Select(i => new
            {
                id = i.Id,
                file_name = i.FileName,
                width = i.Width,
                height = i.Height
            }),
            annotations = dataset.Annotations.Select(a => new
            {
                id = a.Id,
                image_id = a.ImageId,
                category_id = a.CategoryId,
                bbox = a.Bbox,
                area = a.Bbox[2] * a.Bbox[3], // width * height
                iscrowd = 0
            }),
            categories = dataset.Categories.Select(c => new
            {
                id = c.Id,
                name = c.Name
            })
        };

        var json = JsonSerializer.Serialize(coco, JsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        using var stream = new MemoryStream(bytes);
        var path = $"exports/{exportId}/dataset_coco.json";
        return await _fileStorage.CreateFileAsync(stream, path, "application/json");
    }

    private async Task<string> GenerateYolo(ExportDataset dataset, Guid exportId)
    {
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Group annotations by image
            var annotationsByImage = dataset.Annotations
                .GroupBy(a => a.ImageId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var image in dataset.Images)
            {
                var lines = new List<string>();

                if (annotationsByImage.TryGetValue(image.Id, out var annotations))
                {
                    foreach (var ann in annotations)
                    {
                        // Convert to YOLO normalized format
                        // YOLO: category_id is 0-indexed
                        var classId = ann.CategoryId - 1;

                        double xCenter, yCenter, wNorm, hNorm;

                        if (image.Width > 0 && image.Height > 0)
                        {
                            xCenter = (ann.Bbox[0] + ann.Bbox[2] / 2.0) / image.Width;
                            yCenter = (ann.Bbox[1] + ann.Bbox[3] / 2.0) / image.Height;
                            wNorm = ann.Bbox[2] / image.Width;
                            hNorm = ann.Bbox[3] / image.Height;
                        }
                        else
                        {
                            // Fallback: use absolute values (not normalized)
                            xCenter = ann.Bbox[0] + ann.Bbox[2] / 2.0;
                            yCenter = ann.Bbox[1] + ann.Bbox[3] / 2.0;
                            wNorm = ann.Bbox[2];
                            hNorm = ann.Bbox[3];
                        }

                        lines.Add($"{classId} {xCenter:F6} {yCenter:F6} {wNorm:F6} {hNorm:F6}");
                    }
                }

                // Create txt file with same name as image but .txt extension
                var txtFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".txt";
                var entry = archive.CreateEntry(txtFileName);

                using var writer = new StreamWriter(entry.Open());
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }

            // Add classes.txt file
            var classesEntry = archive.CreateEntry("classes.txt");
            using var classesWriter = new StreamWriter(classesEntry.Open());
            foreach (var category in dataset.Categories.OrderBy(c => c.Id))
            {
                await classesWriter.WriteLineAsync(category.Name);
            }
        }

        zipStream.Position = 0;
        var path = $"exports/{exportId}/dataset_yolo.zip";
        return await _fileStorage.CreateFileAsync(zipStream, path, "application/zip");
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static List<ConsensusObject>? ParseConsensusPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);

            if (doc.RootElement.TryGetProperty("objects", out var objectsElement))
            {
                var objects = new List<ConsensusObject>();

                foreach (var obj in objectsElement.EnumerateArray())
                {
                    objects.Add(new ConsensusObject
                    {
                        Label = obj.GetProperty("label").GetString() ?? "",
                        X = obj.GetProperty("x").GetDouble(),
                        Y = obj.GetProperty("y").GetDouble(),
                        W = obj.GetProperty("w").GetDouble(),
                        H = obj.GetProperty("h").GetDouble()
                    });
                }

                return objects;
            }
        }
        catch (JsonException)
        {
            // Invalid payload, skip
        }

        return null;
    }

    private static (int Width, int Height) ParseImageDimensions(string metadata)
    {
        try
        {
            using var doc = JsonDocument.Parse(metadata);
            var width = 0;
            var height = 0;

            if (doc.RootElement.TryGetProperty("width", out var w))
                width = w.GetInt32();
            if (doc.RootElement.TryGetProperty("height", out var h))
                height = h.GetInt32();

            return (width, height);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static string ExtractFileName(string storageUri)
    {
        try
        {
            var uri = new Uri(storageUri);
            return Path.GetFileName(uri.LocalPath);
        }
        catch
        {
            return storageUri;
        }
    }

    private static ExportJobResponse MapToResponse(ExportJob export) => new()
    {
        ExportId = export.ExportId,
        ProjectId = export.ProjectId,
        InitiatorId = export.InitiatorId,
        Format = export.Format,
        Status = export.Status.ToString(),
        FileUri = export.FileUri,
        CreatedAt = export.CreatedAt
    };

    // ─── Internal Models ──────────────────────────────────────────────

    private class ExportDataset
    {
        public List<ExportImage> Images { get; set; } = new();
        public List<ExportAnnotation> Annotations { get; set; } = new();
        public List<ExportCategory> Categories { get; set; } = new();
    }

    private class ExportImage
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private class ExportAnnotation
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public int CategoryId { get; set; }
        public double[] Bbox { get; set; } = null!;
    }

    private class ExportCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    private class ConsensusObject
    {
        public string Label { get; set; } = null!;
        public double X { get; set; }
        public double Y { get; set; }
        public double W { get; set; }
        public double H { get; set; }
    }
}
