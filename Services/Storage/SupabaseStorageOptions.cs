namespace DataLabel_Project_BE.Services.Storage;

public class SupabaseStorageOptions
{
    /// <summary>
    /// Base URL of your Supabase project, e.g. https://xyzcompany.supabase.co
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Service role key or anon key with storage permissions.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default bucket name to upload files to.
    /// </summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// When true the returned upload URL will be the public object URL for the bucket.
    /// </summary>
    public bool UsePublicUrl { get; set; } = true;
}