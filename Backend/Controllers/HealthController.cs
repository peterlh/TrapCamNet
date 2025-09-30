using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check endpoint called");
        return Ok("Healthy");
    }

    [HttpGet("test-error")]
    [AllowAnonymous]
    public IActionResult TestError()
    {
        try
        {
            _logger.LogInformation("Test error endpoint called");
            // Generate a deliberate error for testing
            throw new System.Exception("This is a test error for Serilog");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Test error generated for Serilog testing");
            return StatusCode(500, "Error logged to Serilog");
        }
    }
}
