using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Services
{
    /// <summary>
    /// Service for handling image processing, storage, and animal detection
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Gets a list of image URLs for the specified user
        /// </summary>
        /// <param name="userId">Optional user ID to filter images</param>
        /// <returns>Collection of image URLs</returns>
        Task<IEnumerable<string>> GetImagesAsync(string? userId = null);
        
        /// <summary>
        /// Gets a paginated list of images with details for the specified user
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="userId">Optional user ID to filter images</param>
        /// <returns>Paginated collection of image details</returns>
        Task<(IEnumerable<object> Images, int TotalCount, int CamerasCount)> GetImagesWithDetailsAsync(int page = 1, int pageSize = 12, string? userId = null);
        
        /// <summary>
        /// Processes and uploads an image from base64 string
        /// </summary>
        /// <param name="cameraId">ID of the camera associated with the image</param>
        /// <param name="imageBase64">Base64-encoded image data</param>
        /// <returns>S3 key of the uploaded image, or null if processing failed</returns>
        Task<string?> ProcessAndUploadImageAsync(Guid cameraId, string? imageBase64);
        
        /// <summary>
        /// Uploads a binary image to storage
        /// </summary>
        /// <param name="cameraId">ID of the camera associated with the image</param>
        /// <param name="imageData">Binary image data</param>
        /// <returns>S3 key of the uploaded image, or null if upload failed</returns>
        Task<string?> UploadImageAsync(Guid cameraId, byte[] imageData);
        
        /// <summary>
        /// Creates and saves an image entity with the provided information
        /// </summary>
        /// <param name="camera">The camera that captured the image</param>
        /// <param name="s3Key">S3 key where the image is stored</param>
        /// <param name="emailDate">Date of the email containing the image</param>
        /// <returns>The saved image entity</returns>
        Task<Image> SaveImageEntityAsync(Camera camera, string s3Key, DateTime emailDate);
        
        /// <summary>
        /// Detects animals in an image and updates the image entity
        /// </summary>
        /// <param name="image">The image entity to update</param>
        /// <param name="imageData">Binary image data for detection</param>
        /// <param name="camera">The camera that captured the image, used for location information</param>
        /// <returns>Updated image entity with detected animals</returns>
        Task<Image> DetectAndSaveAnimalsAsync(Image image, byte[] imageData, Camera? camera = null);
        
        /// <summary>
        /// Detects animals in a base64 encoded image and updates the image entity
        /// </summary>
        /// <param name="image">The image entity to update</param>
        /// <param name="imageBase64">Base64-encoded image data for detection</param>
        /// <param name="camera">The camera that captured the image, used for location information</param>
        /// <returns>Updated image entity with detected animals</returns>
        Task<Image> DetectAndSaveAnimalsAsync(Image image, string imageBase64, Camera? camera = null);
    }
}
