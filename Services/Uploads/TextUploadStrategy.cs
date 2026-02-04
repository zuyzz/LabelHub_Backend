using Microsoft.AspNetCore.Http;

namespace DataLabel_Project_BE.Services.Uploads;

public class TextUploadStrategy : IFileUploadStrategy
{
    private readonly Services.Storage.IFileStorage _storage;

    private static readonly string[] TextTypes = new[] { "text/plain", "application/json", "application/xml" };

    public TextUploadStrategy(Services.Storage.IFileStorage storage)
    {
        _storage = storage;
    }

    public bool CanHandle(IFormFile file)
    {
        var ct = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        return TextTypes.Contains(ct) || ext == ".txt" || ext == ".json" || ext == ".xml";
    }

    public async Task<FileProcessResult> ProcessAsync(IFormFile file, Guid projectId, string datasetName)
    {
        // Store the text file as a single scope item. Consumers can parse content later if desired.
        var folder = Path.Combine($"project-{projectId}", datasetName, Guid.NewGuid().ToString());
        await _storage.EnsureFolderAsync(folder);

        var filename = Path.GetFileName(file.FileName!);
        var path = Path.Combine(folder, filename).Replace('\\', '/');

        using var stream = file.OpenReadStream();
        var uri = await _storage.UploadFileAsync(stream, path, file.ContentType ?? "text/plain");

        var item = new FileItem(filename, file.ContentType ?? "text/plain", uri);
        return new FileProcessResult(new[] { item }, Path.Combine($"project-{projectId}", datasetName).Replace('\\', '/'));
    }
}