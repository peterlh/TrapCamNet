using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for location-related operations
/// </summary>
public class LocationService : ILocationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LocationService> _logger;
    private readonly ICountryService _countryService;

    public LocationService(
        AppDbContext context,
        ILogger<LocationService> logger,
        ICountryService countryService)
    {
        _context = context;
        _logger = logger;
        _countryService = countryService;
    }

    /// <summary>
    /// Gets all locations for a user with optional search filter
    /// </summary>
    public async Task<IEnumerable<LocationResponseDto>> GetLocationsForUserAsync(string userId, string? search = null)
    {
        var query = _context.Locations.Where(l => l.UserId == userId).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(search) || 
                                    l.Note.ToLower().Contains(search));
        }
        
        var locations = await query.OrderBy(l => l.Name)
                              .Select(l => new LocationResponseDto
                              {
                                  Id = l.Id,
                                  Name = l.Name,
                                  Note = l.Note,
                                  Latitude = l.Lat,
                                  Longitude = l.Long,
                                  CountryCode = l.CountryCode,
                                  Created = l.Created,
                                  Updated = l.Updated,
                                  UserId = l.UserId
                              })
                              .ToListAsync();
        
        // Add country names to the response
        foreach (var location in locations)
        {
            location.CountryName = _countryService.GetCountryName(location.CountryCode);
        }
                               
        return locations;
    }
    
    /// <summary>
    /// Gets a location by ID
    /// </summary>
    public async Task<Location?> GetLocationByIdAsync(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            _logger.LogWarning("Location not found. ID: {LocationId}", id);
        }
        return location;
    }
    
    /// <summary>
    /// Converts a Location entity to a LocationResponseDto
    /// </summary>
    public LocationResponseDto ConvertToDto(Location location)
    {
        return new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Note = location.Note,
            Latitude = location.Lat,
            Longitude = location.Long,
            Created = location.Created,
            Updated = location.Updated,
            UserId = location.UserId
        };
    }
    
    /// <summary>
    /// Creates a new location
    /// </summary>
    public async Task<LocationResponseDto> CreateLocationAsync(LocationCreateDto locationDto, string userId)
    {
        // Determine country code if not provided
        string? countryCode = locationDto.CountryCode;
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = _countryService.GetCountryCodeForCoordinates(locationDto.Latitude, locationDto.Longitude);
        }
        
        var location = new Location
        {
            Name = locationDto.Name,
            Note = locationDto.Note ?? string.Empty,
            Lat = locationDto.Latitude,
            Long = locationDto.Longitude,
            CountryCode = countryCode,
            UserId = userId,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new location with ID: {LocationId} for user {UserId}", location.Id, userId);

        var createdLocationDto = new LocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Note = location.Note,
            Latitude = location.Lat,
            Longitude = location.Long,
            CountryCode = location.CountryCode,
            CountryName = _countryService.GetCountryName(location.CountryCode),
            Created = location.Created,
            Updated = location.Updated,
            UserId = location.UserId
        };

        return createdLocationDto;
    }
    
    /// <summary>
    /// Updates an existing location
    /// </summary>
    public async Task<bool> UpdateLocationAsync(Guid id, LocationUpdateDto locationDto)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            _logger.LogWarning("Location not found. ID: {LocationId}", id);
            return false;
        }

        location.Name = locationDto.Name;
        location.Note = locationDto.Note ?? location.Note;
        location.Lat = locationDto.Latitude;
        location.Long = locationDto.Longitude;
        
        // Update country code or determine from coordinates if not provided
        if (!string.IsNullOrEmpty(locationDto.CountryCode))
        {
            location.CountryCode = locationDto.CountryCode;
        }
        else if (location.CountryCode == null)
        {
            // Only update if not already set
            location.CountryCode = _countryService.GetCountryCodeForCoordinates(locationDto.Latitude, locationDto.Longitude);
        }
        
        location.Updated = DateTime.UtcNow;

        _context.Entry(location).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated location with ID: {LocationId}", id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating location with ID: {LocationId}", id);
            if (!await LocationExistsAsync(id))
            {
                return false;
            }
            throw;
        }
    }
    
    /// <summary>
    /// Deletes a location
    /// </summary>
    public async Task<(bool success, string? errorMessage)> DeleteLocationAsync(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);
        
        if (location == null)
        {
            _logger.LogWarning("Location not found. ID: {LocationId}", id);
            return (false, "Location not found");
        }
        
        // Check if any cameras are using this location
        var hasRelatedCameras = await HasRelatedCamerasAsync(id);
        
        if (hasRelatedCameras)
        {
            _logger.LogWarning("Cannot delete location {LocationId} because it is associated with one or more cameras", id);
            return (false, "Cannot delete location because it is associated with one or more cameras");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted location with ID: {LocationId}", id);
        return (true, null);
    }
    
    /// <summary>
    /// Checks if any cameras are using this location
    /// </summary>
    public async Task<bool> HasRelatedCamerasAsync(Guid locationId)
    {
        return await _context.Cameras.AnyAsync(c => c.LocationId == locationId);
    }
    
    /// <summary>
    /// Checks if a location exists
    /// </summary>
    public async Task<bool> LocationExistsAsync(Guid id)
    {
        return await _context.Locations.AnyAsync(e => e.Id == id);
    }
}
