using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Models.Requests;

public class LocationUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Note { get; set; }
    
    [Required]
    public decimal Latitude { get; set; }
    
    [Required]
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., DK, US, GB)
    /// If not provided, it will be determined from coordinates
    /// </summary>
    [MaxLength(2)]
    public string? CountryCode { get; set; }
}
