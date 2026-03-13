namespace DataLabelProject.Business.Services.FileUpload.Metadata;

public class MetadataExtractorFactory
{
    private readonly IEnumerable<IMetadataExtractor> _extractors;

    public MetadataExtractorFactory(IEnumerable<IMetadataExtractor> extractors)
    {
        _extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
    }

    /// <summary>
    /// Extracts metadata from the stream using the appropriate extractor for the given content type.
    /// Stream position is preserved when finished.
    /// </summary>
    public async Task<string?> ExtractMetadataAsync(Stream stream, string contentType)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(contentType));
        if (extractor == null) return null;

        long savedPosition = stream.CanSeek ? stream.Position : -1;
        if (stream.CanSeek) stream.Position = 0;

        try
        {
            return await extractor.ExtractAsync(stream);
        }
        catch
        {
            return null;
        }
        finally
        {
            if (savedPosition >= 0 && stream.CanSeek)
                stream.Position = savedPosition;
        }
    }
}
