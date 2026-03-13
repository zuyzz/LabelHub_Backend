using Microsoft.AspNetCore.Http;
using DataLabelProject.Business.Services.FileUpload.Metadata;

namespace DataLabelProject.Business.Services.FileUpload;

public class ImageUploadStrategy : IFileUploadStrategy
{
    private readonly Storage.IFileStorage _storage;
    private readonly MetadataExtractorFactory _metadataExtractorFactory;

    public ImageUploadStrategy(Storage.IFileStorage storage, MetadataExtractorFactory metadataExtractorFactory)
    {
        _storage = storage;
        _metadataExtractorFactory = metadataExtractorFactory;
    }

    public bool CanHandle(IFormFile file)
    {
        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        var isImage = MediaTypeConstants.Image.SupportedContentTypes.Contains(contentType);
        return isImage && !IsArchive(file.FileName);
    }

    public async Task<FileProcessResult> ProcessAsync(
        IFormFile file,
        Guid datasetId,
        string datasetName,
        string mediaType = "image")
    {
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(file.FileName);
        var filename = $"{fileId}{extension}";

        var baseFolder = $"datasets/{datasetId}";
        var path = $"{baseFolder}/{filename}";

        using var stream = file.OpenReadStream();
        var contentType = file.ContentType ?? "application/octet-stream";

        var metadata = await _metadataExtractorFactory.ExtractMetadataAsync(stream, contentType);
        var uri = await _storage.CreateFileAsync(stream, path, contentType);

        var item = new FileItem(filename, contentType, uri, metadata ?? "");

        return new FileProcessResult(
            new[] { item },
            $"[{datasetId}] {datasetName}");
    }

    private static bool IsArchive(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return MediaTypeConstants.Archive.SupportedExtensions.Contains(ext);
    }
}
