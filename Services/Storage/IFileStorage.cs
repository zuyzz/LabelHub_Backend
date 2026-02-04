namespace DataLabel_Project_BE.Services.Storage;

public interface IFileStorage
{
    /// <summary>
    /// Upload a file stream and return a public storage URI (or path) where it can be accessed.
    /// </summary>
    Task<string> UploadFileAsync(Stream content, string path, string contentType);

    /// <summary>
    /// Ensure a folder path exists (if applicable).
    /// </summary>
    Task EnsureFolderAsync(string folderPath);
}