using System.Text.Json;

namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Interface for extracting metadata from media files.
/// </summary>
public interface IMetadataExtractor
{
    /// <summary>
    /// Determines if this extractor can handle the provided file type.
    /// </summary>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <returns>True if this extractor can handle the file type.</returns>
    bool CanHandle(string contentType);

    /// <summary>
    /// Extracts metadata from a stream by reading only header bytes.
    /// </summary>
    /// <param name="stream">The file stream to extract metadata from.</param>
    /// <param name="maxBytesRead">Maximum header bytes to read (default 8KB).</param>
    /// <returns>JSON string representation of the metadata, or null if extraction fails.</returns>
    Task<string?> ExtractAsync(Stream stream, int maxBytesRead = 8192);
}
