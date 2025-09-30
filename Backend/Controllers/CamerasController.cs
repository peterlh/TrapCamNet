using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;
using TrapCam.Backend.Services;
using Amazon.S3;
using Amazon.S3.Model;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CamerasController : ControllerBase
{
    private readonly ILogger<CamerasController> _logger;
    private readonly IS3Service _s3Service;
    private readonly IUserContext _userContext;
    private readonly ICameraService _cameraService;

    public CamerasController(ILogger<CamerasController> logger, IS3Service s3Service, IUserContext userContext, ICameraService cameraService)
    {
        _logger = logger;
        _s3Service = s3Service;
        _userContext = userContext;
        _cameraService = cameraService;
    }

    // GET: api/cameras
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Camera>>> GetCameras([FromQuery] string? search = null)
    {
        // Get the current user ID from the user context service
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        var cameras = await _cameraService.GetCamerasForUserAsync(userId, search);
        return Ok(cameras);
    }

    // GET: api/cameras/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Camera>> GetCamera(Guid id)
    {
        // Get the current user ID from the user context service
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        var camera = await _cameraService.GetCameraByIdAsync(id);

        if (camera == null)
        {
            return NotFound();
        }
        
        // Check if user owns this camera
        if (camera.UserId != userId)
        {
            return Forbid("You don't have access to this camera");
        }

        return Ok(camera);
    }

    // POST: api/cameras
    [HttpPost]
    public async Task<ActionResult<Camera>> CreateCamera(Models.Requests.CameraCreateDto cameraDto)
    {
        // Get the current user ID from the user context service
        var userId = _userContext.UserId;
        
        var camera = await _cameraService.CreateCameraAsync(cameraDto, userId!);

        return CreatedAtAction(nameof(GetCamera), new { id = camera.Id }, camera);
    }

    // PUT: api/cameras/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCamera(Guid id, Models.Requests.CameraUpdateDto cameraDto)
    {
        // Get the current user ID from the user context service
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        // Check if camera exists and user owns it
        var camera = await _cameraService.GetCameraByIdAsync(id);
        if (camera == null)
        {
            return NotFound();
        }
        
        if (camera.UserId != userId)
        {
            return Forbid("You don't have access to this camera");
        }
        
        var success = await _cameraService.UpdateCameraAsync(id, cameraDto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/cameras/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCamera(Guid id)
    {
        // Get the current user ID from the user context service
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        // Check if camera exists and user owns it
        var camera = await _cameraService.GetCameraByIdAsync(id);
        if (camera == null)
        {
            return NotFound();
        }
        
        if (camera.UserId != userId)
        {
            return Forbid("You don't have access to this camera");
        }
        
        var success = await _cameraService.DeleteCameraAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }



    /// <summary>
    /// Gets all email archives for a camera
    /// </summary>
    /// <param name="id">Camera ID</param>
    /// <returns>List of email archives</returns>
    [HttpGet("{id}/EmailArchive")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EmailArchiveDto>>> GetEmailArchives(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving email archives for camera ID: {CameraId}", id);
            
            var userId = _userContext.UserId;
            if (userId == null)
            {
                return Unauthorized("User ID not found");
            }
            
            var isAdmin = User.IsInRole("Admin");
            
            // Get camera and check authorization
            var camera = await _cameraService.GetCameraByIdAsync(id);
            
            if (camera == null)
            {
                _logger.LogWarning("Camera not found with ID: {CameraId}", id);
                return NotFound($"Camera with ID {id} not found");
            }
            
            // Check if user is authorized (owner or admin)
            bool authorized = camera.UserId == userId || isAdmin;
            if (!authorized)
            {
                _logger.LogWarning("Unauthorized access attempt to camera {CameraId} by user {UserId}", id, userId);
                return Forbid("You don't have access to this camera's emails");
            }
            
            var (foundCamera, emailArchives) = await _cameraService.GetEmailArchivesAsync(id);

            return Ok(emailArchives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email archives for camera ID: {CameraId}", id);
            return StatusCode(500, "An error occurred while retrieving the email archives");
        }
    }

    /// <summary>
    /// Gets the HTML content of an archived email
    /// </summary>
    /// <param name="id">ID of the camera</param>
    /// <param name="emailId">ID of the email archive</param>
    /// <returns>HTML content</returns>
    [HttpGet("{id}/EmailArchive/{emailId}/Content")]
    [Authorize]
    public async Task<IActionResult> GetEmailContent(Guid id, Guid emailId)
    {
        try
        {
            _logger.LogInformation("Retrieving email content for camera ID: {CameraId}, email ID: {EmailId}", id, emailId);
            
            var userId = _userContext.UserId;
            if (userId == null)
            {
                return Unauthorized("User ID not found");
            }
            
            var isAdmin = User.IsInRole("Admin");
            
            // Get camera and check authorization
            var camera = await _cameraService.GetCameraByIdAsync(id);
            
            if (camera == null)
            {
                _logger.LogWarning("Camera not found with ID: {CameraId}", id);
                return NotFound($"Camera with ID {id} not found");
            }
            
            // Check if user is authorized (owner or admin)
            bool authorized = camera.UserId == userId || isAdmin;
            if (!authorized)
            {
                _logger.LogWarning("Unauthorized access attempt to camera {CameraId} by user {UserId}", id, userId);
                return Forbid("You don't have access to this camera's emails");
            }
            
            var emailArchive = await _cameraService.GetEmailArchiveAsync(id, emailId);

            if (emailArchive == null)
            {
                _logger.LogWarning("Email with ID {EmailId} not found for camera {CameraId}", emailId, id);
                return NotFound($"Email with ID {emailId} not found for camera {id}");
            }
            
            _logger.LogInformation("Found email archive with S3Key: {S3Key}", emailArchive.S3Key);

        
            string s3Key = emailArchive.S3Key;
            string emailContent;
            string gzKey = s3Key + ".gz";
            _logger.LogInformation("Original key not found, trying with .gz extension: {S3Key}", gzKey);
            
            try
            {
                emailContent = await _s3Service.DownloadTextAsync(
                    _s3Service.EmailArchiveBucket,
                    gzKey,
                    decompress: true);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to retrieve email content with .gz extension");
                return StatusCode(500, "An error occurred while retrieving the email content");
            }

            return Content(emailContent, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email content for camera ID: {CameraId}, email ID: {EmailId}", id, emailId);
            return StatusCode(500, "An error occurred while retrieving the email content");
        }
    }

    /// <summary>
    /// Gets the image attachment of an archived email
    /// </summary>
    /// <param name="id">ID of the camera</param>
    /// <param name="emailId">ID of the email archive</param>
    /// <returns>Image content</returns>
    [HttpGet("{id}/EmailArchive/{emailId}/Image")]
    public async Task<IActionResult> GetEmailImage(Guid id, Guid emailId)
    {
        try
        {
            var userId = _userContext.UserId;
            if (userId == null)
            {
                return Unauthorized("User ID not found");
            }
            
            var isAdmin = User.IsInRole("Admin");
            
            // Get camera and check authorization
            var camera = await _cameraService.GetCameraByIdAsync(id);
            
            if (camera == null)
            {
                return NotFound($"Camera with ID {id} not found");
            }
            
            // Check if user is authorized (owner or admin)
            bool authorized = camera.UserId == userId || isAdmin;
            if (!authorized)
            {
                return Forbid("You don't have access to this camera's emails");
            }
            
            var emailArchive = await _cameraService.GetEmailArchiveAsync(id, emailId);

            if (emailArchive == null)
            {
                return NotFound("Email archive not found");
            }

            // Check if image exists
            if (string.IsNullOrEmpty(emailArchive.ImageS3Key))
            {
                return NotFound("No image attachment found for this email");
            }

            // Download image
            var imageData = await _s3Service.DownloadBinaryAsync(
                _s3Service.ImageBucket,
                emailArchive.ImageS3Key);

            // Determine content type (simple approach)
            var contentType = "image/jpeg"; // Default
            if (emailArchive.ImageS3Key.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/png";
            }
            else if (emailArchive.ImageS3Key.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            {
                contentType = "image/gif";
            }

            return File(imageData, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email image for camera ID: {CameraId}, email ID: {EmailId}", id, emailId);
            return StatusCode(500, "An error occurred while retrieving the email image");
        }
    }
}
