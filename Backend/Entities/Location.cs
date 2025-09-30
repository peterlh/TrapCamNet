using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Entities;

public class Location : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name  { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Note  { get; set; } = string.Empty;
    
    public decimal Long { get; set; }
    public decimal Lat { get; set; }
    
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., DK, US, GB)
    /// </summary>
    [MaxLength(2)]
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// The ID of the user who owns this location
    /// </summary>
    [MaxLength(128)]
    public string? UserId { get; set; }
}