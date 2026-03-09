using System.Text.Json;

namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Extracts metadata from image files (width, height).
/// Reads only header bytes (8KB) for efficient processing.
/// </summary>
public class ImageMetadataExtractor : IMetadataExtractor
{
    private static readonly string[] ImageTypes = new[] 
    { 
        "image/png", 
        "image/jpeg", 
        "image/jpg",  
        "image/webp" 
    };

    public bool CanHandle(string contentType)
    {
        var ct = contentType?.ToLowerInvariant() ?? string.Empty;
        return ImageTypes.Contains(ct);
    }

    public async Task<string?> ExtractAsync(Stream stream, int maxBytesRead = 8192)
    {
        try
        {
            // Read only header bytes from stream
            byte[] headerBuffer = new byte[Math.Min(maxBytesRead, 8192)];
            int bytesRead = await stream.ReadAsync(headerBuffer, 0, headerBuffer.Length);
            
            if (bytesRead < 8)
                return null;

            // Trim buffer to actual bytes read
            Array.Resize(ref headerBuffer, bytesRead);

            var (width, height) = ExtractDimensionsFromBytes(headerBuffer);
            
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

    private static (int width, int height) ExtractWebpDimensions(byte[] data)
    {
        if (data.Length < 30)
            return (0, 0);

        int pos = 12;

        while (pos < data.Length - 10)
        {
            string chunk = System.Text.Encoding.ASCII.GetString(data, pos, 4);

            // -------- VP8 (lossy) --------
            if (chunk == "VP8 ")
            {
                if (pos + 30 > data.Length) return (0, 0);

                int width = data[pos + 26] | (data[pos + 27] << 8);
                int height = data[pos + 28] | (data[pos + 29] << 8);

                return (width & 0x3FFF, height & 0x3FFF);
            }

            // -------- VP8L (lossless) --------
            if (chunk == "VP8L")
            {
                int b0 = data[pos + 9];
                int b1 = data[pos + 10];
                int b2 = data[pos + 11];
                int b3 = data[pos + 12];

                int width = 1 + (((b1 & 0x3F) << 8) | b0);
                int height = 1 + (((b3 & 0x0F) << 10) | (b2 << 2) | ((b1 & 0xC0) >> 6));

                return (width, height);
            }

            // -------- VP8X (extended) --------
            if (chunk == "VP8X")
            {
                int width = 1 + (data[pos + 12] | (data[pos + 13] << 8) | (data[pos + 14] << 16));
                int height = 1 + (data[pos + 15] | (data[pos + 16] << 8) | (data[pos + 17] << 16));

                return (width, height);
            }

            pos++;
        }

        return (0, 0);
    }
}
