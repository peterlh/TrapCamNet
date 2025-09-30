using System.IO.Compression;

namespace TrapCam.Backend.Services;

/// <summary>
/// Interface for S3 storage service operations
/// </summary>
public interface IS3Service
{
    /// <summary>
    /// Ensures that the required buckets exist with proper configuration
    /// </summary>
    Task EnsureBucketsExistAsync();
    
    /// <summary>
    /// Uploads a text file to the specified bucket
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <param name="content">Text content to upload</param>
    /// <param name="compress">Whether to compress the content before upload</param>
    /// <returns>URL to the uploaded file</returns>
    Task<string> UploadTextAsync(string bucketName, string key, string content, bool compress = true);
    
    /// <summary>
    /// Uploads a binary file to the specified bucket
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <param name="data">Binary data to upload</param>
    /// <returns>URL to the uploaded file</returns>
    Task<string> UploadBinaryAsync(string bucketName, string key, byte[] data);
    
    /// <summary>
    /// Uploads a binary file to the specified bucket with compression
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <param name="data">Binary data to upload</param>
    /// <param name="compress">Whether to compress the data before upload</param>
    /// <returns>URL to the uploaded file</returns>
    Task<string> UploadBinaryAsync(string bucketName, string key, byte[] data, bool compress);
    
    /// <summary>
    /// Downloads text content from the specified bucket and key
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <param name="decompress">Whether to decompress the content after download</param>
    /// <returns>Downloaded text content</returns>
    Task<string> DownloadTextAsync(string bucketName, string key, bool decompress = true);
    
    /// <summary>
    /// Downloads binary data from the specified bucket and key
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <returns>Downloaded binary data</returns>
    Task<byte[]> DownloadBinaryAsync(string bucketName, string key);
    
    /// <summary>
    /// Gets a pre-signed URL for the specified object
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    /// <param name="expiryMinutes">Number of minutes until the URL expires</param>
    /// <param name="useLocalEndpoint">If true, uses localhost:9000 instead of the container endpoint</param>
    /// <returns>Pre-signed URL</returns>
    Task<string> GetPresignedUrlAsync(string bucketName, string key, int expiryMinutes = 60, bool useLocalEndpoint = false);
    
    /// <summary>
    /// Deletes an object from the specified bucket
    /// </summary>
    /// <param name="bucketName">Name of the bucket</param>
    /// <param name="key">Object key (file name)</param>
    Task DeleteObjectAsync(string bucketName, string key);
    
    /// <summary>
    /// Gets the email archive bucket name
    /// </summary>
    string EmailArchiveBucket { get; }
    
    /// <summary>
    /// Gets the image bucket name
    /// </summary>
    string ImageBucket { get; }
}
