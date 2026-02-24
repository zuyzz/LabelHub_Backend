using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Business.Services.FileUpload;

public class ImageUploadStrategy : IFileUploadStrategy
{
    private readonly Storage.IFileStorage _storage;

    private static readonly string[] ImageTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif", "image/webp" };

    public ImageUploadStrategy(Storage.IFileStorage storage)
    {
        _storage = storage;
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

        // Folder structure: [datasetId] datasetName/[random-uuid]/
        var baseFolder = $"[{datasetId}] {datasetName}/{Guid.NewGuid()}";
        var path = $"{baseFolder}/{filename}";

        using var stream = file.OpenReadStream();

        var uri = await _storage.UploadFileAsync(
            stream,
            path,
            file.ContentType ?? "application/octet-stream");

        var item = new FileItem(
            filename,
            file.ContentType ?? "application/octet-stream",
            uri);

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
