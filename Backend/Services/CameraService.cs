using Microsoft.EntityFrameworkCore;
using System.Text;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for camera-related operations
/// </summary>
public class CameraService : ICameraService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CameraService> _logger;

    public CameraService(
        AppDbContext context,
        ILogger<CameraService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Finds a camera by its email address
    /// </summary>
    public async Task<Camera?> FindCameraByEmailAsync(string emailAddress)
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            _logger.LogWarning("Attempted to find camera with empty email address");
            return null;
        }
        
        var camera = await _context.Cameras
            .FirstOrDefaultAsync(c => c.InboundEmailAddress.ToLower() == emailAddress.ToLower());
            
        if (camera == null)
        {
            _logger.LogWarning("No camera found with email address: {EmailAddress}", emailAddress);
        }
        
        return camera;
    }
    
    /// <summary>
    /// Gets all cameras for a user with optional search filter
    /// </summary>
    public async Task<IEnumerable<Camera>> GetCamerasForUserAsync(string userId, string? search = null)
    {
        IQueryable<Camera> query = _context.Cameras
            .Include(c => c.CurrentLocation)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(search) || 
                c.InboundEmailAddress.ToLower().Contains(search));
        }

        return await query.ToListAsync();
    }
    
    /// <summary>
    /// Gets a camera by ID
    /// </summary>
    public async Task<Camera?> GetCameraByIdAsync(Guid id)
    {
        var camera = await _context.Cameras
            .Include(c => c.CurrentLocation)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (camera == null)
        {
            _logger.LogWarning("Camera not found with ID: {CameraId}", id);
            return null;
        }

        return camera;
    }
    
    /// <summary>
    /// Creates a new camera
    /// </summary>
    public async Task<Camera> CreateCameraAsync(CameraCreateDto cameraDto, string userId)
    {
        var camera = new Camera
        {
            Id = Guid.NewGuid(),
            Name = cameraDto.Name,
            LastBatteryState = cameraDto.LastBatteryState,
            LocationId = cameraDto.LocationId,
            InboundEmailAddress = GenerateUniqueEmailAddress(),
            UserId = userId,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        _context.Cameras.Add(camera);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new camera with ID: {CameraId} for user {UserId}", camera.Id, userId);
        return camera;
    }
    
    /// <summary>
    /// Updates an existing camera
    /// </summary>
    public async Task<bool> UpdateCameraAsync(Guid id, CameraUpdateDto cameraDto)
    {
        var camera = await _context.Cameras.FindAsync(id);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to update non-existent camera with ID: {CameraId}", id);
            return false;
        }

        // Update properties but not the InboundEmailAddress
        camera.Name = cameraDto.Name;
        camera.LastBatteryState = cameraDto.LastBatteryState;
        camera.LocationId = cameraDto.LocationId;
        camera.Updated = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated camera with ID: {CameraId}", id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating camera with ID: {CameraId}", id);
            if (!await CameraExistsAsync(id))
            {
                return false;
            }
            throw;
        }
    }
    
    /// <summary>
    /// Deletes a camera
    /// </summary>
    public async Task<bool> DeleteCameraAsync(Guid id)
    {
        var camera = await _context.Cameras.FindAsync(id);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to delete non-existent camera with ID: {CameraId}", id);
            return false;
        }

        _context.Cameras.Remove(camera);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted camera with ID: {CameraId}", id);
        return true;
    }
    
    /// <summary>
    /// Checks if a camera exists
    /// </summary>
    public async Task<bool> CameraExistsAsync(Guid id)
    {
        return await _context.Cameras.AnyAsync(e => e.Id == id);
    }
    
    /// <summary>
    /// Gets email archives for a camera
    /// </summary>
    public async Task<(Camera? camera, IEnumerable<EmailArchiveDto>? emailArchives)> GetEmailArchivesAsync(Guid cameraId)
    {
        // Find camera
        var camera = await _context.Cameras.FindAsync(cameraId);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to get email archives for non-existent camera with ID: {CameraId}", cameraId);
            return (null, null);
        }

        // Get email archives for this camera
        var emailArchives = await _context.EmailArchives
            .Where(e => e.CameraId == cameraId)
            .OrderByDescending(e => e.DateTime)
            .Select(e => new EmailArchiveDto
            {
                Id = e.Id,
                DateTime = e.DateTime,
                FromEmail = e.FromEmail,
                FromName = e.FromName,
                HasImage = !string.IsNullOrEmpty(e.ImageS3Key)
            })
            .ToListAsync();

        return (camera, emailArchives);
    }
    
    /// <summary>
    /// Gets email content for a camera
    /// </summary>
    public async Task<EmailArchive?> GetEmailArchiveAsync(Guid cameraId, Guid emailId)
    {
        // Get email archive from database
        var emailArchive = await _context.EmailArchives
            .Include(e => e.Camera)
            .FirstOrDefaultAsync(e => e.Id == emailId && e.CameraId == cameraId);

        if (emailArchive == null)
        {
            _logger.LogWarning("Email with ID {EmailId} not found for camera {CameraId}", emailId, cameraId);
            return null;
        }

        return emailArchive;
    }
    
    /// <summary>
    /// Updates a camera's battery state percentage
    /// </summary>
    public async Task<bool> UpdateCameraBatteryStateAsync(Guid cameraId, int batteryState)
    {
        var camera = await _context.Cameras.FindAsync(cameraId);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to update battery state for non-existent camera with ID: {CameraId}", cameraId);
            return false;
        }

        // Update battery state - this will use the property setter which updates BatteryInfo
        camera.LastBatteryState = batteryState;
        camera.Updated = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated battery state for camera {CameraId} to {BatteryState}%", cameraId, batteryState);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating battery state for camera {CameraId}", cameraId);
            return false;
        }
    }
    
    /// <summary>
    /// Updates a camera's full battery information
    /// </summary>
    public async Task<bool> UpdateCameraBatteryInfoAsync(Guid cameraId, BatteryInfo batteryInfo)
    {
        var camera = await _context.Cameras.FindAsync(cameraId);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to update battery info for non-existent camera with ID: {CameraId}", cameraId);
            return false;
        }

        // Update battery info
        camera.BatteryInfo = batteryInfo;
        camera.Updated = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated battery info for camera {CameraId}: {BatteryInfo}", 
                cameraId, 
                batteryInfo.Percentage.HasValue ? $"{batteryInfo.Percentage}%" : batteryInfo.Voltage.HasValue ? $"{batteryInfo.Voltage}V" : batteryInfo.RawMatch);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating battery info for camera {CameraId}", cameraId);
            return false;
        }
    }
    
    /// <summary>
    /// Updates a camera's last contact time
    /// </summary>
    public async Task<bool> UpdateCameraLastContactAsync(Guid cameraId, DateTime? contactTime = null)
    {
        var camera = await _context.Cameras.FindAsync(cameraId);
        if (camera == null)
        {
            _logger.LogWarning("Attempted to update last contact time for non-existent camera with ID: {CameraId}", cameraId);
            return false;
        }

        // Set contact time to current UTC time if not specified
        camera.LastContact = contactTime ?? DateTime.UtcNow;
        camera.Updated = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated last contact time for camera {CameraId} to {LastContact}", cameraId, camera.LastContact);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last contact time for camera {CameraId}", cameraId);
            return false;
        }
    }
    
    /// <summary>
    /// Generates a unique email address for a camera
    /// </summary>
    private string GenerateUniqueEmailAddress()
    {
        // Generate a unique ID (8 characters) that's human-readable
        // Using a combination of letters and numbers (base36)
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        
        string uniqueId;
        bool isUnique = false;
        
        do
        {
            // Generate an 8-character unique ID
            var stringBuilder = new StringBuilder(8);
            for (int i = 0; i < 8; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }
            uniqueId = stringBuilder.ToString();
            
            // Check if this ID is already in use
            isUnique = !_context.Cameras.Any(c => c.InboundEmailAddress == $"{uniqueId}@app.trapcam.net");
            
        } while (!isUnique);
        
        return $"{uniqueId}@app.trapcam.net";
    }
}
