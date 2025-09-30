using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Data;

public class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Create a logger factory and get a logger instance
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<AppDbContext>();
    }

    public required DbSet<Camera> Cameras { get; set; }
    public required DbSet<Location> Locations { get; set; }
    public required DbSet<EmailArchive> EmailArchives { get; set; }
    public required DbSet<Animal> Animals { get; set; }
    public required DbSet<Image> Images { get; set; }
    public required DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Add a global value converter for all DateTime properties to ensure they are always UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    
                    _logger.LogInformation("Added UTC DateTime converter for property {PropertyName} on entity {EntityName}", 
                        property.Name, entityType.Name);
                }
            }
        }

        // Configure Camera entity
        modelBuilder.Entity<Camera>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.InboundEmailAddress).IsRequired();
            
            // Set InboundEmailAddress to be unique
            entity.HasIndex(e => e.InboundEmailAddress).IsUnique();
        });

        // Configure Location entity
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });
        
        // Configure EmailArchive entity
        modelBuilder.Entity<EmailArchive>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromEmail).IsRequired();
            entity.Property(e => e.S3Key).IsRequired();
            entity.Property(e => e.DateTime).IsRequired();
            
            // Relationship with Camera
            entity.HasOne(e => e.Camera)
                  .WithMany()
                  .HasForeignKey(e => e.CameraId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure Animal entity
        modelBuilder.Entity<Animal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Class).IsRequired();
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.Family).IsRequired();
            entity.Property(e => e.CommonName).IsRequired();
        });
        
        // Configure Image entity
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.S3Key).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ImageDate).IsRequired();
            entity.Property(e => e.Highlight).IsRequired();
            
            // Relationship with Camera
            entity.HasOne(e => e.Camera)
                  .WithMany()
                  .HasForeignKey("CameraId")
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Many-to-many relationship with Animals
            entity.HasMany(e => e.AnimalsOnImage)
                  .WithMany()
                  .UsingEntity("ImageAnimals");
        });
        
        // Configure Device entity
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.FcmToken).IsRequired().HasMaxLength(512);
            entity.Property(e => e.UserId).IsRequired();
            
            // Many-to-many relationship with Cameras
            entity.HasMany(e => e.SubscribedCameras)
                  .WithMany(c => c.SubscribedDevices)
                  .UsingEntity<DeviceCamera>(
                    j => j
                        .HasOne(dc => dc.Camera)
                        .WithMany()
                        .HasForeignKey(dc => dc.CameraId),
                    j => j
                        .HasOne(dc => dc.Device)
                        .WithMany()
                        .HasForeignKey(dc => dc.DeviceId),
                    j =>
                    {
                        j.HasKey(dc => new { dc.DeviceId, dc.CameraId });
                    });
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        EnsureUtcDateTimes();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        EnsureUtcDateTimes();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var now = DateTime.UtcNow;
            var entity = (BaseEntity)entityEntry.Entity;
            
                if (entityEntry.State == EntityState.Added)
                {
                    entity.Created = now;
                }
                
                entity.Updated = now;
            }
    }
    


    /// <summary>
    /// Ensures all DateTime properties on tracked entities have Kind=Utc
    /// This prevents the "Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'" error
    /// </summary>
    private void EnsureUtcDateTimes()
    {
        // First, get all tracked entities
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
        foreach (var entityEntry in entries)
        {
            // Log the entity type for debugging
            _logger.LogInformation("Processing entity: {EntityType}", entityEntry.Entity.GetType().Name);
            
            // Process all properties of the entity
            foreach (var property in entityEntry.Properties)
            {
                // Handle DateTime properties
                if (property.CurrentValue is DateTime dateTime)
                {
                    _logger.LogInformation("DateTime property: {PropertyName}, Kind: {Kind}, Value: {Value}", 
                        property.Metadata.Name, dateTime.Kind, dateTime);
                    
                    // Always force UTC regardless of current Kind
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        property.CurrentValue = utcDateTime;
                        _logger.LogInformation("Converted to UTC: {PropertyName}, New Kind: {Kind}", 
                            property.Metadata.Name, utcDateTime.Kind);
                    }
                }
            }
            
            // Special handling for specific entity types
            if (entityEntry.Entity is Image image)
            {
                // Ensure ImageDate is UTC
                if (image.ImageDate.Kind != DateTimeKind.Utc)
                {
                    image.ImageDate = DateTime.SpecifyKind(image.ImageDate, DateTimeKind.Utc);
                    _logger.LogInformation("Forced Image.ImageDate to UTC: {ImageDate}", image.ImageDate);
                }
            }
            
            // Ensure BaseEntity timestamps are UTC
            if (entityEntry.Entity is BaseEntity baseEntity)
            {
                if (baseEntity.Created.Kind != DateTimeKind.Utc)
                {
                    baseEntity.Created = DateTime.SpecifyKind(baseEntity.Created, DateTimeKind.Utc);
                    _logger.LogInformation("Forced BaseEntity.Created to UTC: {Created}", baseEntity.Created);
                }
                
                if (baseEntity.Updated.Kind != DateTimeKind.Utc)
                {
                    baseEntity.Updated = DateTime.SpecifyKind(baseEntity.Updated, DateTimeKind.Utc);
                    _logger.LogInformation("Forced BaseEntity.Updated to UTC: {Updated}", baseEntity.Updated);
                }
            }
        }
    }
}
