using System.Collections.Generic;

namespace TrapCam.Backend.Services;

/// <summary>
/// Interface for country-related operations
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Gets the country name for a given country code
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code</param>
    /// <returns>Country name or null if not found</returns>
    string? GetCountryName(string? countryCode);

    /// <summary>
    /// Gets all country codes and names
    /// </summary>
    /// <returns>Dictionary of country codes to names</returns>
    Dictionary<string, string> GetAllCountries();
    
    /// <summary>
    /// Gets the country code for coordinates using reverse geocoding
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <returns>ISO 3166-1 alpha-2 country code or null if not found</returns>
    string? GetCountryCodeForCoordinates(decimal latitude, decimal longitude);
}
