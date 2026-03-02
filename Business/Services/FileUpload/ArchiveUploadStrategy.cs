using Microsoft.AspNetCore.Http;
using SharpCompress.Archives;
using DataLabelProject.Business.Services.FileUpload.Metadata;

namespace DataLabelProject.Business.Services.FileUpload;

/// <summary>
/// Processes archive uploads with production-grade optimizations.
/// - Streams archive to temp file (not RAM)
/// - Extracts metadata from header bytes only (8KB)
/// - Enforces resource limits
/// - Uses bounded parallelism
/// </summary>
public class ArchiveUploadStrategy : IFileUploadStrategy
{
    private readonly Storage.IFileStorage _storage;
    private readonly MetadataExtractorFactory _metadataExtractorFactory;
    
    // Resource limits
    private const long MaxArchiveSize = 500 * 1024 * 1024; // 500 MB
    private const long MaxEntrySize = 100 * 1024 * 1024;   // 100 MB per file
    private const int MaxFileCount = 1000;

    public ArchiveUploadStrategy(Storage.IFileStorage storage, IEnumerable<IMetadataExtractor> metadataExtractors)
    {
        _storage = storage;
        _metadataExtractorFactory = new MetadataExtractorFactory(metadataExtractors);
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
        // Validate archive size upfront
        if (file.Length > MaxArchiveSize)
            throw new InvalidOperationException($"Archive size {file.Length} exceeds maximum {MaxArchiveSize}");

        string? tempArchivePath = null;
        try
        {
            // Stream archive to temporary disk file (not RAM)
            tempArchivePath = Path.Combine(Path.GetTempPath(), $"archive_{Guid.NewGuid()}.tmp");
            using (var fileStream = file.OpenReadStream())
            using (var tempFileStream = File.Create(tempArchivePath))
            {
                await fileStream.CopyToAsync(tempFileStream);
            }

            // Process archive from temp file with bounded parallelism
            var uploaded = new List<FileItem>();
            var baseFolder = $"datasets/{datasetId}";
            
            // Read archive entries from temp file
            using (var fileStream = File.OpenRead(tempArchivePath))
            using (var archive = ArchiveFactory.Open(fileStream))
            {
                var entries = archive.Entries
                    .Where(e => !e.IsDirectory && IsImageExt(Path.GetExtension(e.Key)))
                    .ToList();

                if (!entries.Any())
                    throw new InvalidOperationException("Archive contains no image files");

                // Enforce file count limit
                if (entries.Count > MaxFileCount)
                    throw new InvalidOperationException($"Archive contains {entries.Count} files, exceeds maximum {MaxFileCount}");

                // Process entries with bounded concurrency (4 concurrent uploads)
                using (var semaphore = new SemaphoreSlim(4, 4))
                {
                    var tasks = entries.Select(entry => 
                        ProcessEntryAsync(entry, datasetId, baseFolder, semaphore));
                    
                    uploaded.AddRange(await Task.WhenAll(tasks));
                }
            }

            return new FileProcessResult(
                uploaded,
                $"[{datasetId}] {datasetName}");
        }
        finally
        {
            // Cleanup temp file
            if (tempArchivePath != null && File.Exists(tempArchivePath))
            {
                try { File.Delete(tempArchivePath); } 
                catch { /* ignore cleanup errors */ }
            }
        }
    }

    private async Task<FileItem> ProcessEntryAsync(
        IArchiveEntry entry,
        Guid datasetId,
        string baseFolder,
        SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            using var entryStream = entry.OpenEntryStream();

            // Validate entry size
            if (entryStream.Length > MaxEntrySize)
                throw new InvalidOperationException($"File {entry.Key} exceeds maximum size {MaxEntrySize}");

            var filename = Path.GetFileName(entry.Key);
            var path = $"{baseFolder}/{filename}";
            var contentType = GetContentTypeByExtension(Path.GetExtension(filename));

            // Extract metadata from header bytes only (8KB)
            string? metadata = null;
            try
            {
                metadata = await _metadataExtractorFactory.ExtractMetadataAsync(entryStream);
            }
            catch
            {
                // Swallow metadata extraction errors - item will be created without metadata
            }

            // Reset stream for upload
            if (entryStream.CanSeek)
                entryStream.Seek(0, SeekOrigin.Begin);

            // Upload file
            var uri = await _storage.CreateFileAsync(
                entryStream,
                path,
                contentType);

            return new FileItem(filename, contentType, uri, metadata);
        }
        finally
        {
            semaphore.Release();
        }
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

