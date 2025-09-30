using System.ComponentModel.DataAnnotations;

namespace TrapCam.Backend.Entities;

/// <summary>
/// Represents an image captured by a camera
/// </summary>
public class Image : BaseEntity
{
    /// <summary>
    /// Foreign key for the Camera relationship
    /// </summary>
    public Guid CameraId { get; set; }
    
    /// <summary>
    /// The camera that captured this image
    /// </summary>
    public required Camera Camera { get; set; }
    
    /// <summary>
    /// S3 key for the image
    /// </summary>
    [Required]
    [MaxLength(512)]
    public required string S3Key { get; set; }
    
    /// <summary>
    /// Date and time when the image was captured
    /// </summary>
    private DateTime _imageDate;
    
    public DateTime ImageDate 
    { 
        get => _imageDate; 
        set => _imageDate = value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value; 
    }
    
    /// <summary>
    /// Animals detected in this image
    /// </summary>
    public List<Animal> AnimalsOnImage { get; set; } = new();
    
    /// <summary>
    /// Flag indicating if this image should be highlighted (e.g., contains animals)
    /// </summary>
    public bool Highlight { get; set; }
}