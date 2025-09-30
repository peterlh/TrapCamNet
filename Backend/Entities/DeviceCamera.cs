namespace TrapCam.Backend.Entities;

/// <summary>
/// Join entity for the many-to-many relationship between Device and Camera
/// </summary>
public class DeviceCamera
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;
}
