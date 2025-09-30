using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrapCam.Backend.Services;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILogger<LocationsController> _logger;
    private readonly IUserContext _userContext;
    private readonly ILocationService _locationService;

    public LocationsController(ILogger<LocationsController> logger, IUserContext userContext, ILocationService locationService)
    {
        _logger = logger;
        _userContext = userContext;
        _locationService = locationService;
    }

    // GET: api/locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.Responses.LocationResponseDto>>> GetLocations([FromQuery] string? search = null)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        var locations = await _locationService.GetLocationsForUserAsync(userId, search);
        return Ok(locations);
    }

    // GET: api/locations/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Models.Responses.LocationResponseDto>> GetLocation(Guid id)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        var location = await _locationService.GetLocationByIdAsync(id);

        if (location == null)
        {
            return NotFound();
        }
        
        // Check if user owns this location
        if (location.UserId != userId)
        {
            return Forbid("You don't have access to this location");
        }

        var locationDto = _locationService.ConvertToDto(location);
        return Ok(locationDto);
    }

    // POST: api/locations
    [HttpPost]
    public async Task<ActionResult<Models.Responses.LocationResponseDto>> CreateLocation(Models.Requests.LocationCreateDto locationDto)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        var createdLocationDto = await _locationService.CreateLocationAsync(locationDto, userId);

        return CreatedAtAction(nameof(GetLocation), new { id = createdLocationDto.Id }, createdLocationDto);
    }

    // PUT: api/locations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(Guid id, Models.Requests.LocationUpdateDto locationDto)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        // Check if location exists and user owns it
        var location = await _locationService.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound();
        }
        
        if (location.UserId != userId)
        {
            return Forbid("You don't have access to this location");
        }
        
        var success = await _locationService.UpdateLocationAsync(id, locationDto);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/locations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        var userId = _userContext.UserId;
        if (userId == null)
        {
            return Unauthorized("User ID not found");
        }
        
        // Check if location exists and user owns it
        var location = await _locationService.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound();
        }
        
        if (location.UserId != userId)
        {
            return Forbid("You don't have access to this location");
        }
        
        var (success, errorMessage) = await _locationService.DeleteLocationAsync(id);
        
        if (!success)
        {
            if (errorMessage?.Contains("associated with one or more cameras") == true)
            {
                return BadRequest(errorMessage);
            }
            return NotFound();
        }

        return NoContent();
    }
}
