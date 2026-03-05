using Microsoft.AspNetCore.Http;

namespace DataLabelProject.Business.Services.FileUpload;

public record FileItem(string Name, string ContentType, string StorageUri, string? Metadata = null);

public record FileProcessResult(IEnumerable<FileItem> Items, string StoragePrefix);

public interface IFileUploadStrategy
{
    /// Returns true if this strategy can handle the provided upload file.
    bool CanHandle(IFormFile file);

    /// Process the uploaded file and upload individual items into storage; return list of uploaded items.
    /// Storage folder structure: [datasetId] datasetName/[random-uuid]/
    /// <param name="mediaType">Type of media allowed in dataset: "image", "audio", or "video"</param>
    Task<FileProcessResult> ProcessAsync(IFormFile file, Guid datasetId, string datasetName, string mediaType = "image");
}

