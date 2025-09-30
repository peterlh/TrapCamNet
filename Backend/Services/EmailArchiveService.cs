using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Models.Requests;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for handling email archive operations
/// </summary>
public class EmailArchiveService : IEmailArchiveService
{
    private readonly AppDbContext _context;
    private readonly IS3Service _s3Service;
    private readonly ILogger<EmailArchiveService> _logger;

    public EmailArchiveService(
        AppDbContext context,
        IS3Service s3Service,
        ILogger<EmailArchiveService> logger)
    {
        _context = context;
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Archives an email from the request
    /// </summary>
    public async Task<EmailArchive> ArchiveEmailAsync(AddEmailRequest request, Camera camera, string? imageS3Key)
    {
        // Generate unique key for S3
        var emailKey = $"{camera.Id}/{Guid.NewGuid()}.email.html";
        
        // Upload email body to S3 (compressed)
        await _s3Service.UploadTextAsync(
            _s3Service.EmailArchiveBucket,
            emailKey,
            request.Body,
            compress: true);
        
        // Create EmailArchive entity
        var emailArchive = new EmailArchive
        {
            CameraId = camera.Id,
            DateTime = request.DateTime ?? DateTime.UtcNow,
            S3Key = emailKey,
            ImageS3Key = imageS3Key,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
        };

        // Save to database
        _context.EmailArchives.Add(emailArchive);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Email archived successfully for camera {CameraId} from {FromEmail}", camera.Id, request.FromEmail);
        return emailArchive;
    }
}
