using Microsoft.AspNetCore.Http;
using SharpCompress.Archives;

namespace DataLabelProject.Business.Services.FileUpload;

public class ArchiveUploadStrategy : IFileUploadStrategy
{
    private readonly Storage.IFileStorage _storage;

    public ArchiveUploadStrategy(Storage.IFileStorage storage)
    {
        _storage = storage;
    }

    public bool CanHandle(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        return ext == ".zip" || ext == ".rar";
    }

    public async Task<FileProcessResult> ProcessAsync(
        IFormFile file,
        Guid datasetId,
        string datasetName)
    {
        using var mem = new MemoryStream();
        await file.CopyToAsync(mem);
        mem.Position = 0;

        using var archive = ArchiveFactory.Open(mem);

        var entries = archive.Entries
            .Where(e => !e.IsDirectory && IsImageExt(Path.GetExtension(e.Key)))
            .ToList();

        if (!entries.Any())
            throw new InvalidOperationException("Archive contains no image files");

        var baseFolder = $"datasets/{datasetId}";

        var uploaded = new List<FileItem>();

        foreach (var entry in entries)
        {
            using var entryStream = entry.OpenEntryStream();

            var filename = Path.GetFileName(entry.Key);
            var path = $"{baseFolder}/{filename}";
            var contentType = GetContentTypeByExtension(Path.GetExtension(filename));

            var uri = await _storage.CreateFileAsync(
                entryStream,
                path,
                contentType);

            uploaded.Add(new FileItem(filename, contentType, uri));
        }

        return new FileProcessResult(
            uploaded,
            $"[{datasetId}] {datasetName}");
    }

    private static bool IsImageExt(string ext)
        => new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }
            .Contains(ext.ToLowerInvariant());

    private static string GetContentTypeByExtension(string ext)
        => ext.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}
