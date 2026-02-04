using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace DataLabel_Project_BE.Services.Storage;

public class SupabaseFileStorage : IFileStorage
{
    private readonly HttpClient _http;
    private readonly SupabaseStorageOptions _options;
    private readonly ILogger<SupabaseFileStorage> _logger;

    public SupabaseFileStorage(
        HttpClient http,
        IOptions<SupabaseStorageOptions> options,
        ILogger<SupabaseFileStorage> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream content, string path, string contentType)
    {
        // 🔥 BUFFER STREAM (NO HANG)
        await using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        ms.Position = 0;

        using var streamContent = new StreamContent(ms);
        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);

        streamContent.Headers.Add("x-upsert", "true");

        var escapedPath = string.Join("/",
            path.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

        var requestUri = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.Bucket}/{escapedPath}";

        Console.WriteLine("Uploading to Supabase: {0}", requestUri);
        _logger.LogInformation("Uploading to Supabase: {Uri}", requestUri);

        // using var resp = await _http.PutAsync(requestUri, streamContent);
        // Build request explicitly (IMPORTANT for Supabase)
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = streamContent
        };

        // 🔑 Supabase required headers
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");

        // Send request
        var resp = await _http.SendAsync(request);

        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Supabase upload failed {Status}: {Body}",
                resp.StatusCode, body);
            throw new InvalidOperationException(body);
        }

        return $"{_options.Url.TrimEnd('/')}/storage/v1/object/public/{_options.Bucket}/{escapedPath}";
    }

    public Task EnsureFolderAsync(string folderPath)
    {
        // Supabase uses virtual folders
        return Task.CompletedTask;
    }
}
