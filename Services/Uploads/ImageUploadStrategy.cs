using Microsoft.AspNetCore.Http;

namespace DataLabel_Project_BE.Services.Uploads;

public class ImageUploadStrategy : IFileUploadStrategy
{
    private readonly Services.Storage.IFileStorage _storage;

    private static readonly string[] ImageTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif", "image/webp" };

    public ImageUploadStrategy(Services.Storage.IFileStorage storage)
    {
        _storage = storage;
    }

    public bool CanHandle(IFormFile file)
    {
        var ct = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        return ImageTypes.Contains(ct) && !IsArchive(file.FileName);
    }

    public async Task<FileProcessResult> ProcessAsync(IFormFile file, Guid projectId, string datasetName)
    {
        var folder = Path.Combine($"project-{projectId}", datasetName, Guid.NewGuid().ToString());
        await _storage.EnsureFolderAsync(folder);

        var filename = Path.GetFileName(file.FileName!);
        var path = Path.Combine(folder, filename).Replace('\\', '/');

        using var stream = file.OpenReadStream();
        var uri = await _storage.UploadFileAsync(stream, path, file.ContentType ?? "application/octet-stream");

        var item = new FileItem(filename, file.ContentType ?? "application/octet-stream", uri);
        return new FileProcessResult(new[] { item }, Path.Combine($"project-{projectId}", datasetName).Replace('\\', '/'));
    }

    private static bool IsArchive(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext == ".zip" || ext == ".rar";
    }
}