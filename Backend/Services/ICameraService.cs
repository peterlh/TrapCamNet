using System.Collections.Generic;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for camera-related operations
/// </summary>
public interface ICameraService
{
    /// <summary>
    /// Finds a camera by its email address
    /// </summary>
    /// <param name="emailAddress">Email address to search for</param>
    /// <returns>Camera if found, null otherwise</returns>
    Task<Camera?> FindCameraByEmailAsync(string emailAddress);
    
    /// <summary>
    /// Gets all cameras for a user with optional search filter
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of cameras</returns>
    Task<IEnumerable<Camera>> GetCamerasForUserAsync(string userId, string? search = null);
    
    /// <summary>
    /// Gets a camera by ID
    /// </summary>
    /// <param name="id">Camera ID</param>
    /// <returns>Camera if found, null otherwise</returns>
    Task<Camera?> GetCameraByIdAsync(Guid id);
    
    /// <summary>
    /// Creates a new camera
    /// </summary>
    /// <param name="cameraDto">Camera creation data</param>
    /// <param name="userId">User ID</param>
    /// <returns>Created camera</returns>
    Task<Camera> CreateCameraAsync(CameraCreateDto cameraDto, string userId);
    
    /// <summary>
    /// Updates an existing camera
    /// </summary>
    /// <param name="id">Camera ID</param>
    /// <param name="cameraDto">Camera update data</param>
    /// <returns>True if updated, false if not found</returns>
    Task<bool> UpdateCameraAsync(Guid id, CameraUpdateDto cameraDto);
    
    /// <summary>
    /// Deletes a camera
    /// </summary>
    /// <param name="id">Camera ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteCameraAsync(Guid id);
    
    /// <summary>
    /// Checks if a camera exists
    /// </summary>
    /// <param name="id">Camera ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> CameraExistsAsync(Guid id);
    
    /// <summary>
    /// Gets email archives for a camera
    /// </summary>
    /// <param name="cameraId">Camera ID</param>
    /// <returns>Camera and list of email archives</returns>
    Task<(Camera? camera, IEnumerable<EmailArchiveDto>? emailArchives)> GetEmailArchivesAsync(Guid cameraId);
    
    /// <summary>
    /// Gets email content for a camera
    /// </summary>
    /// <param name="cameraId">Camera ID</param>
    /// <param name="emailId">Email ID</param>
    /// <returns>Email archive if found, null otherwise</returns>
    Task<EmailArchive?> GetEmailArchiveAsync(Guid cameraId, Guid emailId);
    
    /// <summary>
    /// Updates a camera's battery state percentage
    /// </summary>
    /// <param name="cameraId">Camera ID</param>
    /// <param name="batteryState">New battery state percentage</param>
    /// <returns>True if updated, false if camera not found</returns>
    Task<bool> UpdateCameraBatteryStateAsync(Guid cameraId, int batteryState);
    
    /// <summary>
    /// Updates a camera's full battery information
    /// </summary>
    /// <param name="cameraId">Camera ID</param>
    /// <param name="batteryInfo">New battery information</param>
    /// <returns>True if updated, false if camera not found</returns>
    Task<bool> UpdateCameraBatteryInfoAsync(Guid cameraId, BatteryInfo batteryInfo);
    
    /// <summary>
    /// Updates a camera's last contact time
    /// </summary>
    /// <param name="cameraId">Camera ID</param>
    /// <param name="contactTime">Contact time (defaults to UTC now if not specified)</param>
    /// <returns>True if updated, false if camera not found</returns>
    Task<bool> UpdateCameraLastContactAsync(Guid cameraId, DateTime? contactTime = null);
}
