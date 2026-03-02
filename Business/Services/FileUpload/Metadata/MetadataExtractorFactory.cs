using System;
using System.IO;

namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Factory for selecting the appropriate metadata extractor based on file type
/// and performing header-only extraction from streams.
/// </summary>
public class MetadataExtractorFactory
{
    private static readonly int MaxHeaderBytes = 8192; // read only header bytes
    private readonly IEnumerable<IMetadataExtractor> _extractors;

    public MetadataExtractorFactory(IEnumerable<IMetadataExtractor> extractors)
    {
        _extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
    }

    /// <summary>
    /// Gets the appropriate metadata extractor for the given content type.
    /// </summary>
    public IMetadataExtractor? GetExtractor(string contentType)
    {
        return _extractors.FirstOrDefault(e => e.CanHandle(contentType));
    }

    /// <summary>
    /// Reads header bytes from the stream and passes them to the extractor.
    /// Stream position is reset to previous value when finished.
    /// </summary>
    public async Task<string?> ExtractMetadataAsync(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        long originalPosition = 0;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            using var headerStream = new MemoryStream();
            byte[] buffer = new byte[MaxHeaderBytes];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                return null;

            await headerStream.WriteAsync(buffer, 0, bytesRead);
            headerStream.Position = 0;

            foreach (var extractor in _extractors)
            {
                try
                {
                    var md = await extractor.ExtractAsync(headerStream, bytesRead);
                    if (md != null)
                        return md;
                }
                catch
                {
                    // ignore extractor errors
                }
            }

            return null;
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = originalPosition;
        }
    }
}
