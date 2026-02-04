using Microsoft.AspNetCore.Http;

namespace DataLabel_Project_BE.Services.Uploads;

public record FileItem(string Name, string ContentType, string StorageUri);

public record FileProcessResult(IEnumerable<FileItem> Items, string StoragePrefix);

public interface IFileUploadStrategy
{
    /// Returns true if this strategy can handle the provided upload file.
    bool CanHandle(IFormFile file);

    /// Process the uploaded file and upload individual items into storage; return list of uploaded items.
    Task<FileProcessResult> ProcessAsync(IFormFile file, Guid projectId, string datasetName);
}