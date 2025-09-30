using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Models.Responses;
using TrapCam.Backend.Services;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InboundController : ControllerBase
{
    private readonly IEmailArchiveService _emailArchiveService;
    private readonly IImageService _imageService;
    private readonly ICameraService _cameraService;
    private readonly IBatteryExtractionService _batteryExtractionService;
    private readonly ILogger<InboundController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public InboundController(
        IEmailArchiveService emailArchiveService,
        IImageService imageService,
        ICameraService cameraService,
        IBatteryExtractionService batteryExtractionService,
        ILogger<InboundController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _emailArchiveService = emailArchiveService;
        _imageService = imageService;
        _cameraService = cameraService;
        _batteryExtractionService = batteryExtractionService;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Adds an email to the archive for a specific camera
    /// </summary>
    /// <param name="request">Email details</param>
    /// <returns>Result of the operation</returns>
    [HttpPost("addemail")]
    [AllowAnonymous]
    public async Task<IActionResult> AddEmail([FromBody] Models.Requests.AddEmailRequest request)
    {
        _logger.LogInformation("Request contains datetime: {X}", request.DateTime);
        
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.ToEmail) || string.IsNullOrEmpty(request.FromEmail) || string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("Required fields are missing");
            }

            // Find camera by email address using the camera service
            var camera = await _cameraService.FindCameraByEmailAsync(request.ToEmail);

            if (camera == null)
            {
                _logger.LogWarning("Attempted to add email for non-existent camera email: {ToEmail}", request.ToEmail);
                return NotFound($"No camera found with email: {request.ToEmail}");
            }
            
            // Process and upload image if provided
            string? imageKey = null;
            byte[]? imageData = null;
            
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                // Remove potential "data:image/jpeg;base64," prefix
                string base64Data = request.ImageBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                }
                
                // Convert base64 to binary
                imageData = Convert.FromBase64String(base64Data);
                imageKey = await _imageService.ProcessAndUploadImageAsync(camera.Id, request.ImageBase64);
            }
            else if (request.Image != null && request.Image.Length > 0)
            {
                imageData = request.Image;
                imageKey = await _imageService.UploadImageAsync(camera.Id, request.Image);
            }
            
            // Save image entity if we have an image
            Image? savedImage = null;
            if (imageKey != null)
            {
                // Use email date for the image date or default to current UTC time
                DateTime emailDate;
                
                if (request.DateTime.HasValue)
                {
                    // Always treat incoming DateTime values as UTC regardless of their Kind
                    // This is necessary because JSON serialization/deserialization loses the Kind information
                    emailDate = DateTime.SpecifyKind(request.DateTime.Value, DateTimeKind.Utc);
                }
                else
                {
                    _logger.LogWarning("No date specified, using UTC Now.");
                    emailDate = DateTime.UtcNow;
                }
                
                // Save image entity using ImageService
                savedImage = await _imageService.SaveImageEntityAsync(camera, imageKey, emailDate);
                
                // Call SpeciesNet for animal detection if configured
                if (imageData != null)
                {
                    // Use ImageService for animal detection with camera for location info
                    savedImage = await _imageService.DetectAndSaveAnimalsAsync(savedImage, imageData, camera);
                }
                else if (!string.IsNullOrEmpty(request.ImageBase64))
                {
                    // Use ImageService for animal detection with base64 image and camera for location info
                    savedImage = await _imageService.DetectAndSaveAnimalsAsync(savedImage, request.ImageBase64, camera);
                }
            }
            
            // Try to extract battery information from the email content
            var batteryInfo = _batteryExtractionService.ExtractBatteryInfo(request.Body);
            if (batteryInfo != null)
            {
                _logger.LogInformation("Battery information extracted: {RawMatch}", batteryInfo.RawMatch);
                
                // Update camera with full battery information
                await _cameraService.UpdateCameraBatteryInfoAsync(camera.Id, batteryInfo);
                
           
            }
            
            // Update camera's last contact time
            await _cameraService.UpdateCameraLastContactAsync(camera.Id);
            _logger.LogInformation("Updated last contact time for camera {CameraId}", camera.Id);
            
            // Archive the email using the service
            var emailArchive = await _emailArchiveService.ArchiveEmailAsync(request, camera, imageKey);

            _logger.LogInformation("Email archived successfully for camera {CameraId} from {FromEmail}", camera.Id, request.FromEmail);
            return Ok(new { id = emailArchive.Id, message = "Email archived successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving email");
            return StatusCode(500, "An error occurred while archiving the email");
        }
    }

    // Email archive endpoints have been moved to CamerasController

    // The DetectAnimalsOnImage method and related model classes have been removed and their functionality moved to the ImageService
}
