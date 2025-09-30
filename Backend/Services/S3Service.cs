using System.IO.Compression;
using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using TrapCam.Backend.Settings;

namespace TrapCam.Backend.Services;

/// <summary>
/// Implementation of the S3 storage service
/// </summary>
public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Settings _settings;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, IOptions<S3Settings> settings, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets the email archive bucket name
    /// </summary>
    public string EmailArchiveBucket => _settings.EmailArchiveBucket;

    /// <summary>
    /// Gets the image bucket name
    /// </summary>
    public string ImageBucket => _settings.ImageBucket;

    /// <summary>
    /// Ensures that the required buckets exist with proper configuration
    /// </summary>
    public async Task EnsureBucketsExistAsync()
    {
        await EnsureBucketExistsAsync(_settings.EmailArchiveBucket, _settings.EmailArchiveTTLDays);
        await EnsureBucketExistsAsync(_settings.ImageBucket, _settings.ImageTTLDays);
    }

    /// <summary>
    /// Uploads a text file to the specified bucket
    /// </summary>
    public async Task<string> UploadTextAsync(string bucketName, string key, string content, bool compress = true)
    {
        try
        {
            byte[] data;
            
            if (compress)
            {
                // Compress the content
                data = CompressText(content);
                key = $"{key}.gz"; // Add .gz extension for compressed content
            }
            else
            {
                data = Encoding.UTF8.GetBytes(content);
            }

            return await UploadBinaryAsync(bucketName, key, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading text to S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Uploads a binary file to the specified bucket
    /// </summary>
    public async Task<string> UploadBinaryAsync(string bucketName, string key, byte[] data)
    {
        return await UploadBinaryAsync(bucketName, key, data, false);
    }
    
    /// <summary>
    /// Uploads a binary file to the specified bucket with optional compression
    /// </summary>
    public async Task<string> UploadBinaryAsync(string bucketName, string key, byte[] data, bool compress)
    {
        try
        {
            byte[] dataToUpload = data;
            
            if (compress)
            {
                using var msi = new MemoryStream(data);
                using var mso = new MemoryStream();
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                dataToUpload = mso.ToArray();
                key = $"{key}.gz"; // Add .gz extension for compressed content
                _logger.LogInformation("Compressing binary data for upload: {BucketName}/{Key}", bucketName, key);
            }
            
            using var memoryStream = new MemoryStream(dataToUpload);
            
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = key,
                BucketName = bucketName,
                ContentType = GetContentType(key)
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            // Return the URL to the uploaded file
            return await GetPresignedUrlAsync(bucketName, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading binary data to S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Downloads text content from the specified bucket and key
    /// </summary>
    public async Task<string> DownloadTextAsync(string bucketName, string key, bool decompress = true)
    {
        try
        {
            byte[] fileData;
            bool isCompressed = key.EndsWith(".gz", StringComparison.OrdinalIgnoreCase);
            
            try
            {
                _logger.LogInformation("Downloading from S3: {BucketName}/{Key}", bucketName, key);
                fileData = await DownloadBinaryAsync(bucketName, key);
            }
            catch (AmazonS3Exception ex) when (ex.Message.Contains("key does not exist"))
            {
                // If the key doesn't exist and we're not already trying a .gz file,
                // try appending .gz to the key
                if (!isCompressed)
                {
                    string gzKey = key + ".gz";
                    _logger.LogInformation("Key not found, trying with .gz extension: {BucketName}/{Key}", bucketName, gzKey);
                    fileData = await DownloadBinaryAsync(bucketName, gzKey);
                    isCompressed = true;
                }
                else
                {
                    // If we're already trying with .gz and it still doesn't exist, rethrow
                    _logger.LogError(ex, "File not found even with .gz extension: {BucketName}/{Key}", bucketName, key);
                    throw;
                }
            }
            
            // Determine if we need to decompress the content
            if (decompress && isCompressed)
            {
                _logger.LogInformation("Decompressing content from: {Key}", key);
                return DecompressText(fileData);
            }
            
            return Encoding.UTF8.GetString(fileData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading text from S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Downloads binary data from the specified bucket and key
    /// </summary>
    public async Task<byte[]> DownloadBinaryAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var memoryStream = new MemoryStream();
            
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading binary data from S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Gets a pre-signed URL for the specified object
    /// </summary>
    /// <param name="bucketName">The bucket name</param>
    /// <param name="key">The object key</param>
    /// <param name="expiryMinutes">URL expiry time in minutes</param>
    /// <param name="useLocalEndpoint">If true, uses localhost:9000 instead of the container endpoint</param>
    /// <returns>A presigned URL for accessing the object</returns>
    public Task<string> GetPresignedUrlAsync(string bucketName, string key, int expiryMinutes = 60, bool useLocalEndpoint = false)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Verb = HttpVerb.GET
            };

            // Determine which S3 client to use
            IAmazonS3 client = useLocalEndpoint ? CreateLocalS3Client() : _s3Client;
            
            // Get the presigned URL from the S3 client
            var presignedUrl = client.GetPreSignedURL(request);
            
            if(useLocalEndpoint){
                presignedUrl = presignedUrl.Replace("https://", "http://");
            }

            // Log the presigned URL for debugging
            _logger.LogDebug("Presigned URL: {PresignedUrl}", presignedUrl);
            
            return Task.FromResult(presignedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pre-signed URL from S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Deletes an object from the specified bucket
    /// </summary>
    public async Task DeleteObjectAsync(string bucketName, string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting object from S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }

    /// <summary>
    /// Creates a detached S3 client that uses localhost:9000 instead of the container endpoint
    /// This is useful for testing presigned URLs that need to be accessible outside the container
    /// </summary>
    /// <returns>A new S3 client configured to use localhost:9000</returns>
    private IAmazonS3 CreateLocalS3Client()
    {
        // Create a new S3 config that's identical to the one used for the main client
        // but with the ServiceURL pointing to localhost instead of the container name
        var s3Config = new AmazonS3Config
        {
            ServiceURL = "http://localhost:9000",  // Use localhost instead of minio:9000
            ForcePathStyle = true,
            UseHttp = true
        };

        // Create a new client with the local config
        return new AmazonS3Client(
            _settings.AccessKey,
            _settings.SecretKey,
            s3Config
        );
    }
    
    #region Private Helper Methods

    /// <summary>
    /// Ensures that a bucket exists with the specified TTL configuration
    /// </summary>
    private async Task EnsureBucketExistsAsync(string bucketName, int ttlDays)
    {
        try
        {
            var bucketExists = await DoesBucketExistAsync(bucketName);
            
            if (!bucketExists)
            {
                // Create the bucket
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };
                
                await _s3Client.PutBucketAsync(putBucketRequest);
                _logger.LogInformation("Created S3 bucket: {BucketName}", bucketName);
                
                // Configure lifecycle policy for TTL
                await ConfigureBucketLifecycleAsync(bucketName, ttlDays);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", bucketName);
            throw;
        }
    }

    /// <summary>
    /// Checks if a bucket exists
    /// </summary>
    private async Task<bool> DoesBucketExistAsync(string bucketName)
    {
        try
        {
            var response = await _s3Client.ListBucketsAsync();
            return response.Buckets.Any(b => b.BucketName == bucketName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Configures a bucket lifecycle policy for automatic deletion after TTL days
    /// </summary>
    private async Task ConfigureBucketLifecycleAsync(string bucketName, int ttlDays)
    {
        try
        {
            var request = new PutLifecycleConfigurationRequest
            {
                BucketName = bucketName,
                Configuration = new LifecycleConfiguration
                {
                    Rules = new List<LifecycleRule>
                    {
                        new LifecycleRule
                        {
                            Id = $"{bucketName}-expiration",
                            Status = LifecycleRuleStatus.Enabled,
                            Expiration = new LifecycleRuleExpiration
                            {
                                Days = ttlDays
                            }
                        }
                    }
                }
            };
            
            await _s3Client.PutLifecycleConfigurationAsync(request);
            _logger.LogInformation("Configured lifecycle policy for bucket {BucketName} with TTL of {TtlDays} days", bucketName, ttlDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring lifecycle policy for bucket: {BucketName}", bucketName);
            throw;
        }
    }

    /// <summary>
    /// Compresses text content using GZip
    /// </summary>
    private byte[] CompressText(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            msi.CopyTo(gs);
        }
        return mso.ToArray();
    }
    


    /// <summary>
    /// Decompresses GZip compressed text
    /// </summary>
    private string DecompressText(byte[] data)
    {
        using var msi = new MemoryStream(data);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }
        return Encoding.UTF8.GetString(mso.ToArray());
    }

    /// <summary>
    /// Gets the content type based on file extension
    /// </summary>
    private string GetContentType(string key)
    {
        var extension = Path.GetExtension(key).ToLowerInvariant();
        
        return extension switch
        {
            ".html" or ".htm" => "text/html",
            ".txt" => "text/plain",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".gz" => "application/gzip",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
