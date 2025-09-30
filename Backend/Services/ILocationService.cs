using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Interface for location-related operations
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets all locations for a user with optional search filter
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of location DTOs</returns>
    Task<IEnumerable<LocationResponseDto>> GetLocationsForUserAsync(string userId, string? search = null);
    
    /// <summary>
    /// Gets a location by ID
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>Location entity if found, null otherwise</returns>
    Task<Location?> GetLocationByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new location
    /// </summary>
    Task<LocationResponseDto> CreateLocationAsync(LocationCreateDto locationDto, string userId);
    
    /// <summary>
    /// Updates an existing location
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="locationDto">Location update data</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateLocationAsync(Guid id, LocationUpdateDto locationDto);
    
    /// <summary>
    /// Deletes a location
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>Success status and error message if applicable</returns>
    Task<(bool success, string? errorMessage)> DeleteLocationAsync(Guid id);
    
    /// <summary>
    /// Checks if a location exists
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> LocationExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if any cameras are using this location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>True if cameras are using this location</returns>
    Task<bool> HasRelatedCamerasAsync(Guid locationId);
    
    /// <summary>
    /// Converts a Location entity to a LocationResponseDto
    /// </summary>
    /// <param name="location">Location entity</param>
    /// <returns>Location DTO</returns>
    LocationResponseDto ConvertToDto(Location location);
}
