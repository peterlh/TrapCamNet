namespace TrapCam.Backend.Settings;

/// <summary>
/// Settings for S3 storage service
/// </summary>
public class S3Settings
{
    /// <summary>
    /// S3 service URL
    /// </summary>
    public string ServiceUrl { get; set; } = "http://minio:9000";
    
    /// <summary>
    /// S3 access key
    /// </summary>
    public string AccessKey { get; set; } = "minioadmin";
    
    /// <summary>
    /// S3 secret key
    /// </summary>
    public string SecretKey { get; set; } = "minioadmin";
    
    /// <summary>
    /// Whether to force path style addressing (required for MinIO)
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;
    
    /// <summary>
    /// Name of the email archive bucket
    /// </summary>
    public string EmailArchiveBucket { get; set; } = "emailarchive";
    
    /// <summary>
    /// Name of the image bucket
    /// </summary>
    public string ImageBucket { get; set; } = "image";
    
    /// <summary>
    /// TTL in days for email archive objects
    /// </summary>
    public int EmailArchiveTTLDays { get; set; } = 3;
    
    /// <summary>
    /// TTL in days for image objects
    /// </summary>
    public int ImageTTLDays { get; set; } = 3;

    /// <summary>
    /// Whether to use HTTP instead of HTTPS
    /// </summary>
    public bool UseHttp { get; set; } = true;
}
