namespace DataLabelProject.Business.Services.FileUpload.Metadata;

public interface IMetadataExtractor
{
    bool CanHandle(string contentType);
    Task<string?> ExtractAsync(Stream stream);
}
