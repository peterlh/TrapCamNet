using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Models.Requests;

/// <summary>
/// Request model for updating device camera subscriptions
/// </summary>
public class DeviceSubscriptionRequest
{
    /// <summary>
    /// List of camera IDs to subscribe to
    /// </summary>
    [Required]
    public required List<Guid> CameraIds { get; set; }
    
    /// <summary>
    /// When true, the device will only receive notifications for images that contain detected animals
    /// </summary>
    public bool NotifyOnlyOnAnimalDetection { get; set; }
}
