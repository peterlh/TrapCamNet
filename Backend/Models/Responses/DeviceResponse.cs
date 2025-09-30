namespace TrapCam.Backend.Models.Responses;

/// <summary>
/// Response model for device information
/// </summary>
public class DeviceResponse
{
    /// <summary>
    /// Device ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// User-friendly name for the device
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Firebase Cloud Messaging token for the device
    /// </summary>
    public required string FcmToken { get; set; }
    
    /// <summary>
    /// List of camera subscriptions
    /// </summary>
    public required List<CameraSubscriptionResponse> Subscriptions { get; set; }
}
