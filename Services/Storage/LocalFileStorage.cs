using Microsoft.AspNetCore.Hosting;

namespace DataLabel_Project_BE.Services.Storage;

/// <summary>
/// Local storage implementation for development/testing. Stores files under wwwroot/storage/{folderPath}.
/// Replace or add a Supabase implementation when ready.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadFileAsync(Stream content, string path, string contentType)
    {
        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "storage", path.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(fullPath) ?? string.Empty;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);

        // Return relative path that can be served by the app: /storage/...
        var relative = "/storage/" + path.Replace('\\', '/');
        return relative;
    }

    public Task EnsureFolderAsync(string folderPath)
    {
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "storage", folderPath.Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return Task.CompletedTask;
    }
}