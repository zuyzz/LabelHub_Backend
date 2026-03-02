using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace DataLabelProject.Business.Services.Storage;

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

    public async Task<string> CreateFileAsync(Stream content, string path, string contentType)
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

    public async Task DeleteFileAsync(string storageUriOrPath)
    {
        // If a full public URL was provided, extract the object path
        var escapedPath = storageUriOrPath;
        var basePublic = $"{_options.Url.TrimEnd('/')}/storage/v1/object/public/{_options.Bucket}/";
        if (storageUriOrPath.StartsWith(basePublic, StringComparison.OrdinalIgnoreCase))
        {
            escapedPath = storageUriOrPath.Substring(basePublic.Length);
        }

        // ensure encoding
        var requestUri = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.Bucket}/{escapedPath}";

        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);

        var resp = await _http.SendAsync(request);
        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Supabase delete file failed {Status}: {Body}", resp.StatusCode, body);
            throw new InvalidOperationException(body);
        }
    }

    public async Task DeleteFolderAsync(string folderPrefix)
    {
        var files = await ListFilesAsync(folderPrefix);

        if (!files.Any())
            return;

        var requestUri =
            $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.Bucket}";

        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);

        var bodyObject = new
        {
            prefixes = files
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(bodyObject),
            System.Text.Encoding.UTF8,
            "application/json");

        var resp = await _http.SendAsync(request);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Supabase delete folder failed {Status}: {Body}",
                resp.StatusCode, body);
            throw new InvalidOperationException(body);
        }
    }

    private async Task<List<string>> ListFilesAsync(string folderPrefix)
    {
        var requestUri =
            $"{_options.Url.TrimEnd('/')}/storage/v1/object/list/{_options.Bucket}";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);

        var bodyObject = new
        {
            prefix = folderPrefix.EndsWith("/") 
                ? folderPrefix 
                : folderPrefix + "/",
            limit = 1000,
            offset = 0
        };

        request.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(bodyObject),
            System.Text.Encoding.UTF8,
            "application/json");

        var resp = await _http.SendAsync(request);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(body);

        using var doc = System.Text.Json.JsonDocument.Parse(body);

        var files = new List<string>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("name", out var nameProp))
            {
                var name = nameProp.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    files.Add($"{folderPrefix.TrimEnd('/')}/{name}");
                }
            }
        }

        return files;
    }
}
