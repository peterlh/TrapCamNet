namespace TrapCam.Backend.Models;

/// <summary>
/// Battery information for a camera
/// </summary>
public class BatteryInfo
{
    /// <summary>
    /// Raw text that matched the battery pattern
    /// </summary>
    public string RawMatch { get; set; } = string.Empty;
    
    /// <summary>
    /// Battery percentage if available
    /// </summary>
    public double? Percentage { get; set; }
    
    /// <summary>
    /// Battery voltage if available
    /// </summary>
    public double? Voltage { get; set; }
}
