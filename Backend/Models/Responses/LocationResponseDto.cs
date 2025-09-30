using System;

namespace TrapCam.Backend.Models.Responses;

public class LocationResponseDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Note { get; set; } = string.Empty;
    
    public decimal Latitude { get; set; }
    
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., DK, US, GB)
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Full country name derived from CountryCode
    /// </summary>
    public string? CountryName { get; set; }
    
    public DateTime Created { get; set; }
    
    public DateTime Updated { get; set; }
    
    public string? UserId { get; set; }
}
