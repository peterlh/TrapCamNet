using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrapCam.Backend.Models.Requests;
using TrapCam.Backend.Services;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;
    private readonly IUserContext _userContext;

    public NotificationsController(
        INotificationService notificationService,
        IUserContext userContext,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all devices for the current user
    /// </summary>
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices()
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }

        var devices = await _notificationService.GetUserDevicesAsync(userId);
        return Ok(devices);
    }

    /// <summary>
    /// Register a new device for push notifications
    /// </summary>
    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationRequest request)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }

        var device = await _notificationService.RegisterDeviceAsync(userId, request);
        return Ok(new { id = device.Id, message = "Device registered successfully" });
    }

    /// <summary>
    /// Update device camera subscriptions
    /// </summary>
    [HttpPut("devices/{id}/subscriptions")]
    public async Task<IActionResult> UpdateDeviceSubscriptions(Guid id, [FromBody] DeviceSubscriptionRequest request)
    {
       var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }

        try
        {
            var device = await _notificationService.UpdateDeviceSubscriptionsAsync(userId, id, request);
            return Ok(new { message = "Device subscriptions updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a device
    /// </summary>
    [HttpDelete("devices/{id}")]
    public async Task<IActionResult> DeleteDevice(Guid id)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }

        try
        {
            await _notificationService.DeleteDeviceAsync(userId, id);
            return Ok(new { message = "Device deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
