using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for handling email archive operations
/// </summary>
public interface IEmailArchiveService
{
    /// <summary>
    /// Archives an email from the request
    /// </summary>
    /// <param name="request">Email details</param>
    /// <param name="camera">Camera associated with the email</param>
    /// <param name="imageS3Key">Optional S3 key for the image attachment</param>
    /// <returns>The created EmailArchive entity</returns>
    Task<EmailArchive> ArchiveEmailAsync(AddEmailRequest request, Camera camera, string? imageS3Key);
}
