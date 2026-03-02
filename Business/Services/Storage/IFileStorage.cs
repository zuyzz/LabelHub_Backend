namespace DataLabelProject.Business.Services.Storage;

public interface IFileStorage
{
    /// <summary>
    /// Create a file from a stream and return a public storage URI (or path) where it can be accessed.
    /// </summary>
    Task<string> CreateFileAsync(Stream content, string path, string contentType);

    /// <summary>
    /// Delete a single file by its storage URI or object path.
    /// </summary>
    Task DeleteFileAsync(string storageUriOrPath);

    /// <summary>
    /// Delete all objects under a folder/prefix.
    /// </summary>
    Task DeleteFolderAsync(string folderPrefix);
}
