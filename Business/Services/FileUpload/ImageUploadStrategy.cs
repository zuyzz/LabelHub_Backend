using Microsoft.AspNetCore.Http;
using DataLabelProject.Business.Services.FileUpload.Metadata;

namespace DataLabelProject.Business.Services.FileUpload;

public class ImageUploadStrategy : IFileUploadStrategy
{
    private readonly Storage.IFileStorage _storage;
    private readonly MetadataExtractorFactory _metadataExtractorFactory;

    private static readonly string[] ImageTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif", "image/webp" };

    public ImageUploadStrategy(Storage.IFileStorage storage, IEnumerable<IMetadataExtractor> metadataExtractors)
    {
        _storage = storage;
        _metadataExtractorFactory = new MetadataExtractorFactory(metadataExtractors);
    }

    public bool CanHandle(IFormFile file)
    {
        var ct = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        return ImageTypes.Contains(ct) && !IsArchive(file.FileName);
    }

    public async Task<FileProcessResult> ProcessAsync(
        IFormFile file,
        Guid datasetId,
        string datasetName)
    {
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(file.FileName);
        var filename = $"{fileId}{extension}";

        var baseFolder = $"datasets/{datasetId}";
        var path = $"{baseFolder}/{filename}";

        using var stream = file.OpenReadStream();

        // Extract metadata from header bytes
        string? metadata = null;
        try
        {
            metadata = await _metadataExtractorFactory.ExtractMetadataAsync(stream);
        }
        catch
        {
            // Swallow metadata extraction errors - item will be created without metadata
        }

        // Upload file
        var uri = await _storage.CreateFileAsync(
            stream,
            path,
            file.ContentType ?? "application/octet-stream");

        var item = new FileItem(
            filename,
            file.ContentType ?? "application/octet-stream",
            uri,
            metadata);

        return new FileProcessResult(
            new[] { item },
            $"[{datasetId}] {datasetName}");
    }

    private static bool IsArchive(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext == ".zip" || ext == ".rar";
    }
}
