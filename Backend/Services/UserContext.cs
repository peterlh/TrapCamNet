using Microsoft.AspNetCore.Http;

namespace TrapCam.Backend.Services;

/// <summary>
/// Implementation of IUserContext that extracts user information from the current HTTP context
/// </summary>
public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user's ID from claims
    /// </summary>
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
}
