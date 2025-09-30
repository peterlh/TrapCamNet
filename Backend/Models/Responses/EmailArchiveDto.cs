using System;

namespace TrapCam.Backend.Models.Responses;

/// <summary>
/// DTO for email archive responses
/// </summary>
public class EmailArchiveDto
{
    /// <summary>
    /// ID of the email archive
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Date and time when the email was received
    /// </summary>
    public DateTime DateTime { get; set; }
    
    /// <summary>
    /// Email address of the sender
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the sender
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// Whether the email has an image attachment
    /// </summary>
    public bool HasImage { get; set; }
}
