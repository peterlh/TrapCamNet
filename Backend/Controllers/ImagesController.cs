using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrapCam.Backend.Services;

namespace TrapCam.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImagesController> _logger;
        private readonly IUserContext _userContext;

        public ImagesController(IImageService imageService, ILogger<ImagesController> logger, IUserContext userContext)
        {
            _imageService = imageService;
            _logger = logger;
            _userContext = userContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> ListImages()
        {
            // Get the current user ID from the user context service
            var userId = _userContext.UserId;
            
            _logger.LogInformation("Retrieving images for user {UserId}", userId);
            var images = await _imageService.GetImagesAsync(userId);
            return Ok(images);
        }
        
        /// <summary>
        /// Gets a paginated list of images with details
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated list of images with details</returns>
        [HttpGet("details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetImagesWithDetails([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            // Get the current user ID from the user context service
            var userId = _userContext.UserId;
            
            _logger.LogInformation("Retrieving images with details for user {UserId}, page {Page}, pageSize {PageSize}", 
                userId, page, pageSize);
            
            // Validate input parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > 100) pageSize = 100; // Limit maximum page size
            
            var (images, totalCount, camerasCount) = await _imageService.GetImagesWithDetailsAsync(page, pageSize, userId);
            
            return Ok(new {
                images,
                totalCount,
                camerasCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
    }
}
