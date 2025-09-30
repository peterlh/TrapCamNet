using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TrapCam.Backend.Models;

namespace TrapCam.Backend.Entities;

public class Camera : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string InboundEmailAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Battery information stored as JSON
    /// </summary>
    public string? BatteryInfoJson { get; set; }
    
    /// <summary>
    /// Battery information (transient property, not stored in database)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public BatteryInfo? BatteryInfo
    {
        get => BatteryInfoJson != null ? JsonSerializer.Deserialize<BatteryInfo>(BatteryInfoJson) : null;
        set => BatteryInfoJson = value != null ? JsonSerializer.Serialize(value) : null;
    }
    
    /// <summary>
    /// Legacy battery state percentage (for backward compatibility)
    /// </summary>
    public int LastBatteryState
    {
        get => BatteryInfo?.Percentage.HasValue == true ? (int)Math.Round(BatteryInfo.Percentage.Value) : 0;
        set
        {
            if (BatteryInfo == null)
            {
                BatteryInfo = new BatteryInfo { Percentage = value };
            }
            else
            {
                BatteryInfo.Percentage = value;
            }
        }
    }
    
    /// <summary>
    /// The last time this camera made contact (sent an email)
    /// </summary>
    public DateTime? LastContact { get; set; }
    
    public Guid? LocationId { get; set; }
    public Location? CurrentLocation { get; set; }
    
    
    /// <summary>
    /// The ID of the user who owns this camera
    /// </summary>
    [MaxLength(128)]
    public string? UserId { get; set; }
    
    /// <summary>
    /// Devices subscribed to this camera for notifications
    /// </summary>
    public List<Device> SubscribedDevices { get; set; } = new();
}