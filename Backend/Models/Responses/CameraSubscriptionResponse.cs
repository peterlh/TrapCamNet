namespace TrapCam.Backend.Models.Responses;

/// <summary>
/// Response model for camera subscription information
/// </summary>
public class CameraSubscriptionResponse
{
    /// <summary>
    /// Camera ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Camera name
    /// </summary>
    public required string Name { get; set; }
}
