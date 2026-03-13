namespace DataLabelProject.Business.Services.FileUpload.Metadata;

/// <summary>
/// Constants for supported media types and their extensions.
/// </summary>
public static class MediaTypeConstants
{
    public static class Image
    {
        public static readonly HashSet<string> SupportedContentTypes =
        [
            "image/png",
            "image/jpeg",
            "image/jpg",
            "image/gif",
            "image/webp"
        ];

        public static readonly HashSet<string> SupportedExtensions =
        [
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".webp"
        ];
    }

    public static class Archive
    {
        public static readonly HashSet<string> SupportedExtensions =
        [
            ".zip",
            ".rar"
        ];
    }

    /// <summary>
    /// Maps file extension to MIME content type.
    /// </summary>
    public static string GetContentTypeByExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            // Image types
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",

            // Future: Audio types
            // ".mp3" => "audio/mpeg",
            // ".wav" => "audio/wav",
            // ".flac" => "audio/flac",
            // ".aac" => "audio/aac",
            // ".ogg" => "audio/ogg",
            // ".m4a" => "audio/mp4",

            // Future: Video types
            // ".mp4" => "video/mp4",
            // ".avi" => "video/x-msvideo",
            // ".mov" => "video/quicktime",
            // ".mkv" => "video/x-matroska",
            // ".webm" => "video/webm",
            // ".flv" => "video/x-flv",
            // ".wmv" => "video/x-ms-wmv",

            _ => "application/octet-stream"
        };
    }
}
