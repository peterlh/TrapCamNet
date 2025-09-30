using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Models.Requests;

/// <summary>
/// Request model for registering a device for push notifications
/// </summary>
public class DeviceRegistrationRequest
{
    /// <summary>
    /// Firebase Cloud Messaging token for the device
    /// </summary>
    [Required]
    public required string FcmToken { get; set; }
    
    /// <summary>
    /// User-friendly name for the device
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Optional list of camera IDs to subscribe to
    /// </summary>
    public List<Guid>? CameraIds { get; set; }
    
    /// <summary>
    /// If true, notifications will only be sent when animals are detected in images
    /// </summary>
    public bool NotifyOnlyOnAnimalDetection { get; set; } = false;
}
