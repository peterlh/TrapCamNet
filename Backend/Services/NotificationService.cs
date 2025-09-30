using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for sending notifications to devices using Firebase Cloud Messaging
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly FirebaseMessaging _firebaseMessaging;

    public NotificationService(
        AppDbContext dbContext,
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _logger = logger;

        // Initialize Firebase Admin SDK if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var projectId = configuration["FIREBASE_PROJECT_ID"];
                _logger.LogInformation("Firebase Project ID from configuration: {ProjectId}", projectId);
                
                if (string.IsNullOrEmpty(projectId))
                {
                    _logger.LogWarning("Firebase Project ID not configured. Notifications will not be sent.");
                }
                
                // Check if we have a service account key file path in configuration
                var serviceAccountKeyPath = configuration["FIREBASE_SERVICE_ACCOUNT_KEY_PATH"];
                _logger.LogInformation("Firebase Service Account Key Path: {Path}", serviceAccountKeyPath);
                
                if (!string.IsNullOrEmpty(serviceAccountKeyPath))
                {
                    // Check if the file exists before trying to load it
                    if (System.IO.File.Exists(serviceAccountKeyPath))
                    {
                        _logger.LogInformation("Firebase service account file exists at {Path}", serviceAccountKeyPath);
                        
                        try
                        {
                            // Read the file content to verify it's accessible and valid JSON
                            var fileContent = System.IO.File.ReadAllText(serviceAccountKeyPath);
                            var jsonContent = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fileContent);
                            
                            if (jsonContent != null && jsonContent.TryGetValue("project_id", out var projectIdValue))
                            {
                                _logger.LogInformation("Successfully read service account file. Project ID in file: {ProjectId}", projectIdValue);
                                
                                // Initialize with service account key file and explicit project ID
                                var credential = GoogleCredential.FromFile(serviceAccountKeyPath);
                                _logger.LogInformation("Successfully loaded GoogleCredential from file");
                                
                                FirebaseApp.Create(new AppOptions
                                {
                                    Credential = credential,
                                    ProjectId = projectIdValue.ToString() // Explicitly set the ProjectId from the JSON file
                                });
                                _logger.LogInformation("Firebase Admin SDK initialized with service account key file and ProjectId: {ProjectId}", projectIdValue);
                            }
                            else
                            {
                                _logger.LogWarning("Service account file does not contain project_id field");
                                
                                // Initialize with service account key file without project ID
                                var credential = GoogleCredential.FromFile(serviceAccountKeyPath);
                                _logger.LogInformation("Successfully loaded GoogleCredential from file");
                                
                                FirebaseApp.Create(new AppOptions
                                {
                                    Credential = credential
                                });
                                _logger.LogInformation("Firebase Admin SDK initialized with service account key file but no ProjectId");
                            }
                        }
                        catch (Exception fileEx)
                        {
                            _logger.LogError(fileEx, "Error reading or parsing service account file");
                            throw; // Re-throw to be caught by outer exception handler
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Firebase service account key file not found at {Path}. Falling back to default credentials.", serviceAccountKeyPath);
                        // Fall back to default credentials
                        FirebaseApp.Create(new AppOptions
                        {
                            ProjectId = projectId
                        });
                        _logger.LogInformation("Firebase Admin SDK initialized with application default credentials");
                    }
                }
                else
                {
                    // Initialize with application default credentials
                    _logger.LogInformation("No service account key path provided, using application default credentials with project ID: {ProjectId}", projectId);
                    FirebaseApp.Create(new AppOptions
                    {
                        ProjectId = projectId
                    });
                    _logger.LogInformation("Firebase Admin SDK initialized with application default credentials");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
            }
        }
        else
        {
            _logger.LogInformation("Firebase Admin SDK already initialized. Using existing instance.");
        }

        _firebaseMessaging = FirebaseMessaging.DefaultInstance;
        
        // Log Firebase App details to verify initialization
        var app = FirebaseApp.DefaultInstance;
        _logger.LogInformation("Firebase App details after initialization - Name: {Name}, ProjectId: {ProjectId}", 
            app?.Name ?? "null", 
            app?.Options?.ProjectId ?? "null");
    }

    /// <summary>
    /// Register a device for push notifications
    /// </summary>
    public async Task<Device> RegisterDeviceAsync(string userId, DeviceRegistrationRequest request)
    {
        // Check if device with same token already exists for this user
        var existingDevice = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.FcmToken == request.FcmToken);

        if (existingDevice != null)
        {
            // Update existing device
            existingDevice.Name = request.Name;
            existingDevice.NotifyOnlyOnAnimalDetection = request.NotifyOnlyOnAnimalDetection;
            _logger.LogInformation("Updating existing device {DeviceId} for user {UserId} with NotifyOnlyOnAnimalDetection={NotifyOnlyOnAnimalDetection}", 
                existingDevice.Id, userId, request.NotifyOnlyOnAnimalDetection);
        }
        else
        {
            // Create new device
            existingDevice = new Device
            {
                UserId = userId,
                FcmToken = request.FcmToken,
                Name = request.Name,
                NotifyOnlyOnAnimalDetection = request.NotifyOnlyOnAnimalDetection
            };
            _dbContext.Devices.Add(existingDevice);
            _logger.LogInformation("Creating new device for user {UserId} with NotifyOnlyOnAnimalDetection={NotifyOnlyOnAnimalDetection}", 
                userId, request.NotifyOnlyOnAnimalDetection);
        }

        // Update camera subscriptions if provided
        if (request.CameraIds != null && request.CameraIds.Count > 0)
        {
            // Get cameras that belong to the user
            var userCameras = await _dbContext.Cameras
                .Where(c => c.UserId == userId && request.CameraIds.Contains(c.Id))
                .ToListAsync();

            // Clear existing subscriptions and add new ones
            existingDevice.SubscribedCameras.Clear();
            existingDevice.SubscribedCameras.AddRange(userCameras);
            
            _logger.LogInformation("Updated device with {Count} camera subscriptions", userCameras.Count);
        }

        await _dbContext.SaveChangesAsync();
        return existingDevice;
    }

    /// <summary>
    /// Update device camera subscriptions
    /// </summary>
    public async Task<Device> UpdateDeviceSubscriptionsAsync(string userId, Guid deviceId, DeviceSubscriptionRequest request)
    {
        // Get device
        var device = await _dbContext.Devices
            .Include(d => d.SubscribedCameras)
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId)
            ?? throw new KeyNotFoundException($"Device with ID {deviceId} not found for user {userId}");

        // Get cameras that belong to the user
        var userCameras = await _dbContext.Cameras
            .Where(c => c.UserId == userId && request.CameraIds.Contains(c.Id))
            .ToListAsync();

        // Clear existing subscriptions and add new ones
        device.SubscribedCameras.Clear();
        device.SubscribedCameras.AddRange(userCameras);
        
        // Update notification preference if provided
        device.NotifyOnlyOnAnimalDetection = request.NotifyOnlyOnAnimalDetection;
        
        _logger.LogInformation("Updated device {DeviceId} with {Count} camera subscriptions and notification preference: {NotifyOnlyOnAnimalDetection}", 
            deviceId, userCameras.Count, request.NotifyOnlyOnAnimalDetection);

        await _dbContext.SaveChangesAsync();
        return device;
    }

    /// <summary>
    /// Delete a device
    /// </summary>
    public async Task DeleteDeviceAsync(string userId, Guid deviceId)
    {
        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId)
            ?? throw new KeyNotFoundException($"Device with ID {deviceId} not found for user {userId}");

        _dbContext.Devices.Remove(device);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Deleted device {DeviceId} for user {UserId}", deviceId, userId);
    }

    /// <summary>
    /// Get all devices for a user
    /// </summary>
    public async Task<List<DeviceResponse>> GetUserDevicesAsync(string userId)
    {
        var devices = await _dbContext.Devices
            .Include(d => d.SubscribedCameras)
            .Where(d => d.UserId == userId)
            .ToListAsync();

        return devices.Select(d => new DeviceResponse
        {
            Id = d.Id,
            Name = d.Name,
            FcmToken = d.FcmToken,
            Subscriptions = d.SubscribedCameras.Select(c => new CameraSubscriptionResponse
            {
                Id = c.Id,
                Name = c.Name
            }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Send a notification to devices subscribed to a camera
    /// </summary>
    public async Task<int> SendNewImageNotificationAsync(Camera camera, Image image)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase Admin SDK not initialized. Cannot send notifications.");
            return 0;
        }
        
        _logger.LogInformation("Firebase App details - Name: {Name}, ProjectId: {ProjectId}", 
            FirebaseApp.DefaultInstance.Name, 
            FirebaseApp.DefaultInstance.Options.ProjectId);

        try
        {
            // Get all devices subscribed to this camera
            var devices = await _dbContext.Devices
                .Include(d => d.SubscribedCameras)
                .Where(d => d.SubscribedCameras.Any(c => c.Id == camera.Id))
                .ToListAsync();
                
            // Filter devices based on notification preference
            bool hasAnimals = image.AnimalsOnImage.Count > 0;
            devices = devices.Where(d => !d.NotifyOnlyOnAnimalDetection || (d.NotifyOnlyOnAnimalDetection && hasAnimals)).ToList();
            
            _logger.LogInformation("After filtering by animal detection preference: {Count} devices will receive notifications", devices.Count);

            if (devices.Count == 0)
            {
                _logger.LogInformation("No devices subscribed to camera {CameraId}", camera.Id);
                return 0;
            }

            _logger.LogInformation("Sending notifications to {Count} devices for camera {CameraId}", devices.Count, camera.Id);

            // Prepare notification message
            var message = new MulticastMessage
            {
                Tokens = devices.Select(d => d.FcmToken).ToList(),
                Notification = new Notification
                {
                    Title = $"New image from {camera.Name}",
                    Body = image.AnimalsOnImage.Count > 0 
                        ? $"Animals detected: {string.Join(", ", image.AnimalsOnImage.Select(a => a.CommonName).Distinct())}" 
                        : "New image received"
                },
                Data = new Dictionary<string, string>
                {
                    { "cameraId", camera.Id.ToString() },
                    { "imageId", image.Id.ToString() },
                    { "timestamp", image.ImageDate.ToString("o") }
                }
            };

            // Log the message details before sending
            _logger.LogInformation("Preparing to send notification to {Count} devices with tokens: {Tokens}", 
                message.Tokens.Count, 
                string.Join(", ", message.Tokens.Select(t => t[..Math.Min(10, t.Length)] + "..."))); // Only log part of the token for security
                
            int successCount = 0;
            try
            {
                // Send the message
                _logger.LogInformation("Calling FCM SendEachForMulticastAsync");
                var response = await _firebaseMessaging.SendEachForMulticastAsync(message);
                successCount = response.SuccessCount;
                
                _logger.LogInformation("Notification sent. Success: {SuccessCount}, Failure: {FailureCount}", 
                    response.SuccessCount, response.FailureCount);
                    
                // Log any failures in detail
                if (response.FailureCount > 0 && response.Responses != null)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        var messageResponse = response.Responses[i];
                        if (!messageResponse.IsSuccess)
                        {
                            _logger.LogError("Failed to send notification to token {TokenPrefix}... Error: {Error}",
                                message.Tokens[i][..Math.Min(10, message.Tokens[i].Length)],
                                messageResponse.Exception?.Message ?? "Unknown error");
                        }
                    }
                }
            }
            catch (FirebaseMessagingException fcmEx)
            {
                _logger.LogError(fcmEx, "Firebase Messaging Exception when sending notification. ErrorCode: {ErrorCode}, Message: {Message}",
                    fcmEx.ErrorCode, fcmEx.Message);
                throw;
            }

            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications for camera {CameraId}", camera.Id);
            return 0;
        }
    }
}
