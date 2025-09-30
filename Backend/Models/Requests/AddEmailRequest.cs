using System;

namespace TrapCam.Backend.Models.Requests;

/// <summary>
/// Request model for adding an email to the archive
/// </summary>
public class AddEmailRequest
{
    /// <summary>
    /// Email address of the recipient (camera)
    /// </summary>
    public string ToEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the sender
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the sender
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// Date and time when the email was sent
    /// </summary>
    public DateTime? DateTime { get; set; }
    
    /// <summary>
    /// Email body (HTML)
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Image attachment (base64-encoded string or binary)
    /// </summary>
    public string? ImageBase64 { get; set; }
    
    /// <summary>
    /// Image attachment (binary) - for internal use
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public byte[]? Image { get; set; }
}
