using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Factory for selecting the appropriate metadata extractor based on file type.
/// </summary>
public class MetadataExtractorFactory
{
    private readonly IEnumerable<IMetadataExtractor> _extractors;

    public MetadataExtractorFactory(IEnumerable<IMetadataExtractor> extractors)
    {
        _extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
    }

    /// <summary>
    /// Gets the appropriate metadata extractor for the given file content type.
    /// </summary>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <returns>An IMetadataExtractor instance, or null if no extractor can handle the content type.</returns>
    public IMetadataExtractor? GetExtractor(string contentType)
    {
        return _extractors.FirstOrDefault(e => e.CanHandle(contentType));
    }

    /// <summary>
    /// Extracts metadata from the given file using the appropriate extractor.
    /// </summary>
    /// <param name="file">The file to extract metadata from.</param>
    /// <returns>JSON string representation of the metadata, or null if no extractor is found or extraction fails.</returns>
    public async Task<string?> ExtractMetadataAsync(IFormFile file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        var extractor = GetExtractor(file.ContentType);
        if (extractor == null)
            return null;

        return await extractor.ExtractAsync(file);
    }
}
