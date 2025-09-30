using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrapCam.Backend.Entities;

/// <summary>
/// Represents an archived email for a camera
/// </summary>
public class EmailArchive : BaseEntity
{
    /// <summary>
    /// ID of the camera associated with this email
    /// </summary>
    [Required]
    public Guid CameraId { get; set; }
    
    /// <summary>
    /// The camera associated with this email
    /// </summary>
    [ForeignKey(nameof(CameraId))]
    public Camera? Camera { get; set; }
    
    /// <summary>
    /// Date and time when the email was received
    /// </summary>
    [Required]
    public DateTime DateTime { get; set; }
    
    
    /// <summary>
    /// Email address of the sender
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the sender
    /// </summary>
    [MaxLength(256)]
    public string? FromName { get; set; }
    
    /// <summary>
    /// S3 key for the email body
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string S3Key { get; set; } = string.Empty;
    
    /// <summary>
    /// S3 key for the image attachment (if any)
    /// </summary>
    [MaxLength(512)]
    public string? ImageS3Key { get; set; }
    

}
