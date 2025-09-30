using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Services
{
    /// <summary>
    /// Service for handling image processing, storage, and animal detection
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IS3Service _s3Service;
        private readonly ILogger<ImageService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
    
        private string GetImageUrl(string s3Key)
        {
            try
            {
                // Generate a new presigned URL with the AWS SDK
                var presignedUrl = _s3Service.GetPresignedUrlAsync(_s3Service.ImageBucket, s3Key, 60, true).Result;

                return presignedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate URL for S3 key {S3Key}, using dummy image", s3Key);
                // Return a dummy image URL if S3 service fails
                return "";
            }
        }
        
        public ImageService(
            IS3Service s3Service, 
            ILogger<ImageService> logger,
            AppDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            INotificationService notificationService)
        {
            _s3Service = s3Service;
            _logger = logger;
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets a list of image URLs for the specified user
        /// </summary>
        public Task<IEnumerable<string>> GetImagesAsync(string? userId = null)
        {
            // In a real implementation, we would filter images based on the user ID
            // For now, we'll just return all dummy images regardless of user ID
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }
        
        /// <summary>
        /// Gets a paginated list of images with details for the specified user
        /// </summary>
        public async Task<(IEnumerable<object> Images, int TotalCount, int CamerasCount)> GetImagesWithDetailsAsync(int page = 1, int pageSize = 12, string? userId = null)
        {
            _logger.LogInformation("Getting images with details for page {Page}, pageSize {PageSize}", page, pageSize);
            
            try
            {
                // Get real images from the database with their details
                var query = _dbContext.Images
                    .Include(i => i.Camera)
                    .ThenInclude(c => c.CurrentLocation)
                    .Include(i => i.AnimalsOnImage)
                    .OrderByDescending(i => i.ImageDate);
                
                // Get the total count of images
                var totalCount = await query.CountAsync();
                
                // Calculate the number of items to skip
                var skip = (page - 1) * pageSize;
                
                // Get the paginated results
                var images = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
                
                // Get the count of unique cameras
                var camerasCount = await _dbContext.Images
                    .Select(i => i.CameraId)
                    .Distinct()
                    .CountAsync();
                
                // Map the images to the response format
                var imageDetails = images.Select(image => new
                {
                    id = image.Id,
                    url = GetImageUrl(image.S3Key),
                    name = $"Image {image.Id}",
                    imageDate = image.ImageDate,
                    location = image.Camera?.CurrentLocation?.Name ?? "Unknown Location",
                    cameraId = image.CameraId,
                    cameraName = image.Camera?.Name ?? "Unknown Camera",
                    animals = image.AnimalsOnImage.Select(animal => new
                    {
                        id = animal.Id,
                        commonName = animal.CommonName
                    }).ToList()
                });
                
                return (imageDetails, totalCount, camerasCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images with details");
                throw;
            }
        }
        
        /// <summary>
        /// Processes and uploads an image from base64 string
        /// </summary>
        public async Task<string?> ProcessAndUploadImageAsync(Guid cameraId, string? imageBase64)
        {
            if (string.IsNullOrEmpty(imageBase64))
            {
                _logger.LogWarning("No base64 image data received in the request");
                return null;
            }
            
            try
            {
                _logger.LogInformation("Base64 image data received, length: {Length} characters", imageBase64.Length);
                
                // Remove potential "data:image/jpeg;base64," prefix
                string base64Data = imageBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                    _logger.LogInformation("Removed data URI prefix from base64 string");
                }
                
                // Convert base64 to binary
                byte[] imageData = Convert.FromBase64String(base64Data);
                _logger.LogInformation("Successfully converted base64 image to binary, size: {Size} bytes", imageData.Length);
                
                // Upload the binary image
                return await UploadImageAsync(cameraId, imageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process or convert base64 image");
                return null;
            }
        }
        
        /// <summary>
        /// Uploads a binary image to storage
        /// </summary>
        public async Task<string?> UploadImageAsync(Guid cameraId, byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("No image data provided for upload");
                return null;
            }
            
            try
            {
                _logger.LogInformation("Preparing to upload image, size: {Size} bytes", imageData.Length);
                string imageKey = $"{cameraId}/{Guid.NewGuid()}.image.jpg";
                _logger.LogInformation("Generated image key: {ImageKey}", imageKey);
                
                var imageUrl = await _s3Service.UploadBinaryAsync(
                    _s3Service.ImageBucket,
                    imageKey,
                    imageData);
                
                _logger.LogInformation("Image successfully uploaded to S3 with key: {ImageKey}, URL: {ImageUrl}", imageKey, imageUrl);
                return imageKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image to S3");
                return null;
            }
        }
        
        /// <summary>
        /// Creates and saves an image entity with the provided information
        /// </summary>
        public async Task<Image> SaveImageEntityAsync(Camera camera, string s3Key, DateTime emailDate)
        {
            _logger.LogInformation("Creating new Image entity for camera {CameraId} with S3Key {S3Key}", camera.Id, s3Key);
            
            // Log the incoming DateTime details
            _logger.LogInformation("Incoming emailDate: Value={Value}, Kind={Kind}", 
                emailDate, emailDate.Kind);
            
            // Ensure the DateTime is in UTC format
            DateTime utcEmailDate = emailDate.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(emailDate, DateTimeKind.Utc) : emailDate;
            
            // Log the converted DateTime details
            _logger.LogInformation("Converted utcEmailDate: Value={Value}, Kind={Kind}", 
                utcEmailDate, utcEmailDate.Kind);
            
            var image = new Image
            {
                Camera = camera,
                CameraId = camera.Id,
                S3Key = s3Key,
                ImageDate = utcEmailDate, // Use the email date instead of current time
                AnimalsOnImage = new List<Animal>(),
                Highlight = false
                // Created and Updated are handled by AppDbContext.UpdateTimestamps()
            };
            
            // Log the image entity DateTime fields before saving
            _logger.LogInformation("Image entity before save: ImageDate={ImageDate}, ImageDateKind={ImageDateKind}", 
                image.ImageDate, image.ImageDate.Kind);
            
            _dbContext.Images.Add(image);
            
            try {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved new Image entity with ID {ImageId}", image.Id);
                return image;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error saving image entity. Exception details: {Message}", ex.Message);
                
                // Get all tracked entities with their DateTime properties for debugging
                var entries = _dbContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
                
                foreach (var entry in entries)
                {
                    _logger.LogInformation("Entity: {EntityType}, State: {State}", 
                        entry.Entity.GetType().Name, entry.State);
                    
                    var dateTimeProps = entry.Properties
                        .Where(p => p.Metadata.ClrType == typeof(DateTime) || p.Metadata.ClrType == typeof(DateTime?));
                    
                    foreach (var prop in dateTimeProps)
                    {
                        if (prop.CurrentValue is DateTime dt)
                        {
                            _logger.LogInformation("Property: {PropertyName}, Value: {Value}, Kind: {Kind}", 
                                prop.Metadata.Name, dt, dt.Kind);
                        }
                        else if (prop.CurrentValue is DateTime?)
                        {
                            var nullableDateTime = (DateTime?)prop.CurrentValue;
                            if (nullableDateTime.HasValue)
                            {
                                _logger.LogInformation("Property: {PropertyName}, Value: {Value}, Kind: {Kind}", 
                                    prop.Metadata.Name, nullableDateTime.Value, nullableDateTime.Value.Kind);
                            }
                        }
                    }
                }
                
                throw; // Re-throw the exception after logging
            }
        }
        
        /// <summary>
        /// Detects animals in an image and updates the image entity
        /// </summary>
        public async Task<Image> DetectAndSaveAnimalsAsync(Image image, byte[] imageData, Camera? camera = null)
        {
            try
            {
                // Check if animal recognition is configured
                string? animalRecognitionServer = _configuration["ANIMAL_RECOGNITION_SERVER"];
                if (string.IsNullOrEmpty(animalRecognitionServer))
                {
                    _logger.LogInformation("Animal recognition server not configured, skipping detection");
                    return image;
                }
                
                // Get confidence threshold from configuration or use default
                double confidenceThreshold = 10.0; // Default threshold
                if (double.TryParse(_configuration["ANIMAL_DETECTION_CONFIDENCE_THRESHOLD"], out double threshold))
                {
                    confidenceThreshold = threshold;
                }
                
                _logger.LogInformation("Detecting animals on image {ImageId} with confidence threshold {Threshold}", 
                    image.Id, confidenceThreshold);
                
                // Create HTTP client
                var client = _httpClientFactory.CreateClient();
                string detectEndpoint = $"{animalRecognitionServer.TrimEnd('/')}/detect";
                
                // Create multipart form content
                using var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(imageContent, "file", "image.jpg");
                
                // Get country code from camera's location if available
                string countryCode = "DK"; // Default to DK if no location available
                
                if (camera?.CurrentLocation?.CountryCode != null)
                {
                    countryCode = camera.CurrentLocation.CountryCode;
                }
                
                // Add country parameter (required by the API)
                content.Add(new StringContent(countryCode), "country");
                _logger.LogInformation("Using country code {CountryCode} for animal detection at endpoint {Endpoint}", 
                    countryCode, detectEndpoint);
                
                // Log camera and location details for debugging
                if (camera != null)
                {
                    _logger.LogInformation("Camera details - ID: {CameraId}, LocationId: {LocationId}", 
                        camera.Id, camera.LocationId);
                    
                    if (camera.CurrentLocation != null)
                    {
                        _logger.LogInformation("Location details - ID: {LocationId}, Lat: {Latitude}, Long: {Longitude}, CountryCode: {CountryCode}", 
                            camera.CurrentLocation.Id, camera.CurrentLocation.Lat, camera.CurrentLocation.Long, camera.CurrentLocation.CountryCode);
                    }
                }
                
                // Send request to animal recognition service
                var response = await client.PostAsync(detectEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Read the error response to help with debugging
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Animal detection failed with status code {StatusCode}. Error: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    return image;
                }
                
                // Parse response
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the full response for debugging
                _logger.LogInformation("SpeciesNet API response for image {ImageId}: {Response}", 
                    image.Id, responseContent);
                    
                var detectionResponse = JsonSerializer.Deserialize<DetectionResponse>(responseContent);
                
                if (detectionResponse?.Detections == null || !detectionResponse.Detections.Any())
                {
                    _logger.LogInformation("No animals detected in image {ImageId}", image.Id);
                    return image;
                }
                
                _logger.LogInformation("Received {Count} animal detections for image {ImageId}", 
                    detectionResponse.Detections.Count, image.Id);
                
                // Process detections
                bool hasHighlightedAnimals = false;
                foreach (var detection in detectionResponse.Detections)
                {
                    // Skip detections below confidence threshold
                    if (detection.Confidence < confidenceThreshold)
                    {
                        _logger.LogInformation("Skipping detection with confidence {Confidence} below threshold {Threshold}",
                            detection.Confidence, confidenceThreshold);
                        continue;
                    }
                    
                    // Check if we already have this animal in our database by UUID
                    Animal? animal = null;
                    
                    if (!string.IsNullOrEmpty(detection.Uuid) && Guid.TryParse(detection.Uuid, out Guid animalId))
                    {
                        // Try to find existing animal by UUID as Id
                        animal = await _dbContext.Animals
                            .FirstOrDefaultAsync(a => a.Id == animalId);
                            
                        if (animal != null)
                        {
                            _logger.LogInformation("Found existing animal with UUID: {Uuid}", detection.Uuid);
                        }
                    }
                    
                    // If not found, create a new animal
                    if (animal == null)
                    {
                        // If we have a valid UUID, use it as the Id
                        Guid newAnimalId = Guid.NewGuid();
                        if (!string.IsNullOrEmpty(detection.Uuid) && Guid.TryParse(detection.Uuid, out Guid parsedId))
                        {
                            newAnimalId = parsedId;
                        }
                        
                        animal = new Animal
                        {
                            Id = newAnimalId,
                            Class = detection.ClassName ?? "unknown",
                            Order = detection.TaxaLevel ?? "unknown",
                            Family = detection.TaxaLevel ?? "unknown", // Using taxa_level as family for now
                            CommonName = detection.CommonName ?? "unknown",
                            SpeciesName = detection.SpeciesId.ToString()
                        };
                        
                        // Add the new animal to the database
                        _dbContext.Animals.Add(animal);
                        _logger.LogInformation("Created new animal: {CommonName} with Id: {Id}", 
                            animal.CommonName, animal.Id);
                    }
                    
                    // Add the animal to the image
                    if (!image.AnimalsOnImage.Any(a => a.Id == animal.Id))
                    {
                        image.AnimalsOnImage.Add(animal);
                        hasHighlightedAnimals = true;
                    }
                }
                
                // Update highlight flag if animals were detected
                if (hasHighlightedAnimals)
                {
                    image.Highlight = true;
                }
                
                // Save changes to database
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Updated image {ImageId} with {Count} animals, highlight: {Highlight}", 
                    image.Id, image.AnimalsOnImage.Count, image.Highlight);
                
                // Send notifications to subscribed devices
                if (camera != null)
                {
                    try
                    {
                        // Always send notification - the NotificationService will filter based on device preferences
                        _logger.LogInformation("Sending notifications for image {ImageId} (has animals: {HasAnimals})", 
                            image.Id, hasHighlightedAnimals);
                        await _notificationService.SendNewImageNotificationAsync(camera, image);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notifications for image {ImageId}", image.Id);
                    }
                }
                
                return image;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting animals on image {ImageId}", image.Id);
                return image;
            }
        }
        
        /// <summary>
        /// Detects animals in a base64 encoded image and updates the image entity
        /// </summary>
        public async Task<Image> DetectAndSaveAnimalsAsync(Image image, string imageBase64, Camera? camera = null)
        {
            if (string.IsNullOrEmpty(imageBase64))
            {
                _logger.LogWarning("No base64 image data provided for animal detection");
                return image;
            }
            
            try
            {
                // Remove potential "data:image/jpeg;base64," prefix
                string base64Data = imageBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                }
                
                // Convert base64 to binary
                byte[] imageData = Convert.FromBase64String(base64Data);
                
                // Call the binary version of the method
                return await DetectAndSaveAnimalsAsync(image, imageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing base64 image for animal detection");
                return image;
            }
        }
        
        /// <summary>
        /// Model class for animal detection response
        /// </summary>
        private class DetectionResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
            
            [JsonPropertyName("country")]
            public string? Country { get; set; }
            
            [JsonPropertyName("country_iso3")]
            public string? CountryIso3 { get; set; }
            
            [JsonPropertyName("detections")]
            public List<Detection> Detections { get; set; } = new();
        }
        
        /// <summary>
        /// Model class for individual animal detection
        /// </summary>
        private class Detection
        {
            [JsonPropertyName("uuid")]
            public string? Uuid { get; set; }
            
            [JsonPropertyName("species_id")]
            public int SpeciesId { get; set; }
            
            [JsonPropertyName("common_name")]
            public string? CommonName { get; set; }
            
            [JsonPropertyName("class_name")]
            public string? ClassName { get; set; }
            
            [JsonPropertyName("taxa_level")]
            public string? TaxaLevel { get; set; }
            
            [JsonPropertyName("confidence")]
            public double Confidence { get; set; }
        }
    }
}
