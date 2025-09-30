namespace TrapCam.Backend.Entities;

public class Animal : BaseEntity
{
    public required string Class { get; set; }
    public required string Order { get; set; }
    public required string Family { get; set; }
    public string? Genus { get; set; }
    public string? SpeciesName { get; set; }
    public required string CommonName { get; set; }
}