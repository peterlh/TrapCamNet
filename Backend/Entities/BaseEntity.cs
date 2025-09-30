namespace TrapCam.Backend.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    
    private DateTime _created;
    public DateTime Created 
    { 
        get => _created; 
        set => _created = value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value; 
    }
    
    private DateTime _updated;
    public DateTime Updated 
    { 
        get => _updated; 
        set => _updated = value.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value; 
    }
}