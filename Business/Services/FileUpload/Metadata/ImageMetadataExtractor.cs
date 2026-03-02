using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Extracts metadata from image files (width, height).
/// </summary>
public class ImageMetadataExtractor : IMetadataExtractor
{
    private static readonly string[] ImageTypes = new[] 
    { 
        "image/png", 
        "image/jpeg", 
        "image/jpg", 
        "image/gif", 
        "image/webp" 
    };

    public bool CanHandle(string contentType)
    {
        var ct = contentType?.ToLowerInvariant() ?? string.Empty;
        return ImageTypes.Contains(ct);
    }

    public async Task<string?> ExtractAsync(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var (width, height) = ExtractDimensionsFromBytes(memoryStream.ToArray());
            
            if (width > 0 && height > 0)
            {
                var metadata = new { width, height };
                return JsonSerializer.Serialize(metadata);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts image dimensions by reading file headers.
    /// Supports PNG, JPEG, GIF, WEBP formats.
    /// </summary>
    private static (int width, int height) ExtractDimensionsFromBytes(byte[] data)
    {
        if (data.Length < 8)
            return (0, 0);

        // Check PNG
        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return ExtractPngDimensions(data);

        // Check JPEG
        if (data[0] == 0xFF && data[1] == 0xD8)
            return ExtractJpegDimensions(data);

        // Check GIF
        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
            return ExtractGifDimensions(data);

        // Check WEBP
        if (data.Length >= 12 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && 
            data[3] == 0x46 && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            return ExtractWebpDimensions(data);

        return (0, 0);
    }

    private static (int width, int height) ExtractPngDimensions(byte[] data)
    {
        if (data.Length < 24)
            return (0, 0);

        // PNG dimensions are at bytes 16-24 (big-endian)
        int width = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
        int height = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];

        return (width, height);
    }

    private static (int width, int height) ExtractJpegDimensions(byte[] data)
    {
        int pos = 2;
        while (pos < data.Length - 9)
        {
            // Look for SOF markers (0xFFC0 to 0xFFC3)
            if (data[pos] == 0xFF && (data[pos + 1] >= 0xC0 && data[pos + 1] <= 0xC3))
            {
                int height = (data[pos + 5] << 8) | data[pos + 6];
                int width = (data[pos + 7] << 8) | data[pos + 8];
                return (width, height);
            }

            // Skip to next marker
            if (data[pos] == 0xFF)
                pos += 2;
            else
                pos++;
        }

        return (0, 0);
    }

    private static (int width, int height) ExtractGifDimensions(byte[] data)
    {
        if (data.Length < 10)
            return (0, 0);

        // GIF dimensions are at bytes 6-10 (little-endian)
        int width = data[6] | (data[7] << 8);
        int height = data[8] | (data[9] << 8);

        return (width, height);
    }

    private static (int width, int height) ExtractWebpDimensions(byte[] data)
    {
        if (data.Length < 30)
            return (0, 0);

        // WEBP dimensions are encoded in VP8 bitstream
        // For lossy WEBP, look for VP8 chunk and extract dimensions
        int pos = 12;
        while (pos < data.Length - 10)
        {
            if (data[pos] == 0x56 && data[pos + 1] == 0x50 && data[pos + 2] == 0x38)
            {
                // Found VP8/VP8L chunk, dimensions at offset 6-10
                if (pos + 10 < data.Length)
                {
                    int width = ((data[pos + 9] << 8) | data[pos + 8]) & 0x3FFF;
                    int height = ((data[pos + 7] << 8) | data[pos + 6]) & 0x3FFF;
                    return (width + 1, height + 1); // VP8 stores width-1, height-1
                }
            }
            pos++;
        }

        return (0, 0);
    }
}
