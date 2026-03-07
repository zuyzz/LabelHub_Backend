using System.Text.Json;
using DataLabelProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;

namespace DataLabelProject.Business.Services.Export;

public class ExportService
{
    private readonly AppDbContext _context;

    public ExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> ExportToCocoAsync(Guid projectId)
    {
        // Get project
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) throw new Exception("Project not found");

        // Get all datasets for the project
        var datasetIds = await _context.ProjectDatasets
            .Where(pd => pd.ProjectId == projectId)
            .Select(pd => pd.DatasetId)
            .ToListAsync();

        // Get all dataset items
        var datasetItems = await _context.DatasetItems
            .Where(di => datasetIds.Contains(di.DatasetId))
            .ToListAsync();

        // Get all annotation tasks for these datasets
        var taskIds = await _context.AnnotationTasks
            .Where(at => datasetIds.Contains(at.DatasetId))
            .Select(at => at.TaskId)
            .ToListAsync();

        // Get all annotations 
        var annotations = await _context.Annotations
            .Where(a => taskIds.Contains(a.TaskId) && !a.IsDraft)
            .Include(a => a.AnnotationTask)
            .Include(a => a.AnnotationLabelSet)
            .ThenInclude(ls => ls.Labels)
            .ToListAsync();

        // Get unique labels
        var labelIds = annotations.SelectMany(a => a.AnnotationLabelSet.Labels.Select(l => l.LabelId)).Distinct();
        var labels = await _context.Labels
            .Where(l => labelIds.Contains(l.LabelId))
            .Include(l => l.LabelLabelSet)
            .ToListAsync();

        // Build COCO structure
        var coco = new CocoDataset
        {
            Info = new CocoInfo
            {
                Description = $"Exported from project {project.Name}",
                Version = "1.0",
                Year = DateTime.Now.Year,
                Contributor = "LabelHub",
                DateCreated = DateTime.Now.ToString("yyyy-MM-dd")
            },
            Licenses = new List<CocoLicense>(),
            Images = datasetItems.Select((di, index) => new CocoImage
            {
                Id = index + 1,
                FileName = Path.GetFileName(di.StorageUri),
                Width = 0, // TODO: parse from metadata if available
                Height = 0,
                CocoUrl = di.StorageUri,
                DateCaptured = di.CreatedAt?.ToString("yyyy-MM-dd") ?? ""
            }).ToList(),
            Annotations = new List<CocoAnnotation>(),
            Categories = labels.Select((l, index) => new CocoCategory
            {
                Id = index + 1,
                Name = l.Name,
                Supercategory = l.LabelLabelSet?.Name ?? ""
            }).ToList()
        };

        // Map for ids
        var imageIdMap = datasetItems.Select((di, index) => new { di.ItemId, CocoId = index + 1 }).ToDictionary(x => x.ItemId, x => x.CocoId);
        var labelIdMap = labels.Select((l, index) => new { l.LabelId, CocoId = index + 1 }).ToDictionary(x => x.LabelId, x => x.CocoId);

        int annId = 1;
        foreach (var annotation in annotations)
        {
            
            var payload = JsonSerializer.Deserialize<AnnotationPayload>(annotation.AnnotationPayload);
            if (payload == null || payload.Bbox == null) continue;

            
            if (!Guid.TryParse(annotation.AnnotationTask.ScopeUri, out var itemId)) continue;
            if (!imageIdMap.TryGetValue(itemId, out var imageId)) continue;

            var labelId = payload.LabelId;
            if (!labelIdMap.TryGetValue(labelId, out var categoryId)) continue;

            coco.Annotations.Add(new CocoAnnotation
            {
                Id = annId++,
                ImageId = imageId,
                CategoryId = categoryId,
                Bbox = payload.Bbox,
                Area = payload.Bbox[2] * payload.Bbox[3],
                IsCrowd = 0
            });
        }

        return JsonSerializer.Serialize(coco, new JsonSerializerOptions { WriteIndented = true });
    }
}

// COCO classes
public class CocoDataset
{
    public CocoInfo Info { get; set; }
    public List<CocoLicense> Licenses { get; set; }
    public List<CocoImage> Images { get; set; }
    public List<CocoAnnotation> Annotations { get; set; }
    public List<CocoCategory> Categories { get; set; }
}

public class CocoInfo
{
    public string Description { get; set; }
    public string Version { get; set; }
    public int Year { get; set; }
    public string Contributor { get; set; }
    public string DateCreated { get; set; }
}

public class CocoLicense
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}

public class CocoImage
{
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string FileName { get; set; }
    public string CocoUrl { get; set; }
    public string DateCaptured { get; set; }
}

public class CocoAnnotation
{
    public int Id { get; set; }
    public int ImageId { get; set; }
    public int CategoryId { get; set; }
    public List<double> Bbox { get; set; }
    public double Area { get; set; }
    public int IsCrowd { get; set; }
}

public class CocoCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Supercategory { get; set; }
}

public class AnnotationPayload
{
    public Guid LabelId { get; set; }
    public List<double> Bbox { get; set; }
}