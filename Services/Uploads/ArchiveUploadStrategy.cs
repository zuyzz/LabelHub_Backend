using Microsoft.AspNetCore.Http;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace DataLabel_Project_BE.Services.Uploads;

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
        Guid projectId,
        string datasetName)
    {
        using var mem = new MemoryStream();
        await file.CopyToAsync(mem);
        mem.Position = 0;

        using var archive = ArchiveFactory.Open(mem);

        var entries = archive.Entries
            .Where(e => !e.IsDirectory)
            .ToList();

        if (!entries.Any())
            throw new InvalidOperationException("Archive contains no files");

        bool allImages = entries.All(e => IsImageExt(Path.GetExtension(e.Key)));
        bool allText   = entries.All(e => IsTextExt(Path.GetExtension(e.Key)));

        if (!allImages && !allText)
            throw new InvalidOperationException(
                "Archive must contain only images OR only text files");

        var baseFolder =
            $"project-{projectId}/{datasetName}/{Guid.NewGuid()}";

        await _storage.EnsureFolderAsync(baseFolder);

        var uploaded = new List<FileItem>();

        foreach (var entry in entries)
        {
            using var entryStream = entry.OpenEntryStream();

            var filename = Path.GetFileName(entry.Key);
            var path = $"{baseFolder}/{filename}";
            var contentType = GetContentTypeByExtension(
                Path.GetExtension(filename));

            var uri = await _storage.UploadFileAsync(
                entryStream,
                path,
                contentType);

            uploaded.Add(new FileItem(filename, contentType, uri));
        }

        return new FileProcessResult(
            uploaded,
            $"project-{projectId}/{datasetName}");
    }

    private static bool IsImageExt(string ext)
        => new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp" }
            .Contains(ext.ToLowerInvariant());

    private static bool IsTextExt(string ext)
        => new[] { ".txt", ".json", ".xml" }
            .Contains(ext.ToLowerInvariant());

    private static string GetContentTypeByExtension(string ext)
        => ext.ToLowerInvariant() switch
        {
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif"  => "image/gif",
            ".webp" => "image/webp",
            ".json" => "application/json",
            ".xml"  => "application/xml",
            ".txt"  => "text/plain",
            _       => "application/octet-stream"
        };
}
