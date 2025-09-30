using System;
using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Models.Requests;

public class CameraUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int LastBatteryState { get; set; }
    
    public Guid? LocationId { get; set; }
}
