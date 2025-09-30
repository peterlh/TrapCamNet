using System;

namespace TrapCam.Backend.Services;

/// <summary>
/// Provides access to the current user's context information
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user's ID from claims
    /// </summary>
    string? UserId { get; }
}
