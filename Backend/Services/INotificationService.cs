using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for sending notifications to devices
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Register a device for push notifications
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Device registration request</param>
    /// <returns>The registered device</returns>
    Task<Device> RegisterDeviceAsync(string userId, DeviceRegistrationRequest request);
    
    /// <summary>
    /// Update device camera subscriptions
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device ID</param>
    /// <param name="request">Subscription update request</param>
    /// <returns>The updated device</returns>
    Task<Device> UpdateDeviceSubscriptionsAsync(string userId, Guid deviceId, DeviceSubscriptionRequest request);
    
    /// <summary>
    /// Delete a device
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deviceId">Device ID</param>
    Task DeleteDeviceAsync(string userId, Guid deviceId);
    
    /// <summary>
    /// Get all devices for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of devices</returns>
    Task<List<DeviceResponse>> GetUserDevicesAsync(string userId);
    
    /// <summary>
    /// Send a notification to devices subscribed to a camera
    /// </summary>
    /// <param name="camera">Camera</param>
    /// <param name="image">Image</param>
    /// <returns>Number of devices notified</returns>
    Task<int> SendNewImageNotificationAsync(Camera camera, Image image);
}
