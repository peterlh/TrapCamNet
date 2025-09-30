using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Entities;

/// <summary>
/// Represents a user device for push notifications
/// </summary>
public class Device : BaseEntity
{
    /// <summary>
    /// Firebase Cloud Messaging token for this device
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string FcmToken { get; set; } = string.Empty;
    
    /// <summary>
    /// User-friendly name for the device
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The ID of the user who owns this device
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Cameras that this device is subscribed to for notifications
    /// </summary>
    public List<Camera> SubscribedCameras { get; set; } = new();
    
    /// <summary>
    /// If true, notifications will only be sent when animals are detected in images
    /// </summary>
    public bool NotifyOnlyOnAnimalDetection { get; set; } = false;
}
