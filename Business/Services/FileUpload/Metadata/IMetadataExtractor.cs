using Microsoft.AspNetCore.Http;

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
    /// Extracts metadata from the file as a JSON string.
    /// </summary>
    /// <param name="file">The file to extract metadata from.</param>
    /// <returns>JSON string representation of the metadata, or null if extraction fails.</returns>
    Task<string?> ExtractAsync(IFormFile file);
}
