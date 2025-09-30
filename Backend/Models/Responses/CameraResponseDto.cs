using System;

namespace TrapCam.Backend.Models.Responses;

public class CameraResponseDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string InboundEmailAddress { get; set; } = string.Empty;
    
    public int LastBatteryState { get; set; }
    
    public Guid? LocationId { get; set; }
    
    public string? LocationName { get; set; }
    
    public DateTime Created { get; set; }
    
    public DateTime Updated { get; set; }
}
