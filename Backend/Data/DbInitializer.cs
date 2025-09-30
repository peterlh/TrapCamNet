using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using System.IO;
using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Retry logic for database connection
        int maxRetries = 5;
        int retryDelaySeconds = 5;
        
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                if (retry > 0)
                {
                    logger.LogInformation($"Retry attempt {retry} of {maxRetries}");
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
                }
                
                // Check database connection
                logger.LogInformation("Checking database connection");
                var canConnect = await dbContext.Database.CanConnectAsync();
                logger.LogInformation($"Database connection check: {(canConnect ? "Success" : "Failed")}");
                
                if (canConnect)
                {
                    // If we get here, we've successfully connected
                    break;
                }
            }
            catch (Exception ex) when (retry < maxRetries - 1)
            {
                logger.LogWarning(ex, $"Database connection attempt {retry + 1} failed. Retrying in {retryDelaySeconds} seconds...");
            }
        }
        
        try
        {
            // Seed data if needed
            logger.LogInformation("Checking if seed data is needed");
            
            // Check if there are any locations
            var locationCount = await dbContext.Locations.CountAsync();
            logger.LogInformation($"Found {locationCount} locations in database");
            
            if (locationCount == 0)
            {
                logger.LogInformation("No locations found. Seeding initial data...");
                await SeedLocations(dbContext);
                logger.LogInformation("Seeded initial location data");
            }
            else
            {
                logger.LogInformation("Database already contains location data. Skipping location seed operation.");
            }
            
            // Check if there are any animals
            var animalCount = await dbContext.Animals.CountAsync();
            logger.LogInformation($"Found {animalCount} animals in database");
            
            if (animalCount == 0)
            {
                logger.LogInformation("No animals found. Seeding animal taxonomy data...");
                await SeedAnimals(dbContext, logger);
                logger.LogInformation("Seeded animal taxonomy data");
            }
            else
            {
                logger.LogInformation("Database already contains animal data. Skipping animal seed operation.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
    


    private static async Task SeedLocations(AppDbContext dbContext)
    {
        try
        {
            // Create sample locations
            var locations = new List<Location>
            {
                new Location
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Forest Edge",
                    Note = "Northern edge of the forest",
                    Long = 45,
                    Lat = 23,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Location
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "River Bend",
                    Note = "Southern bend of the river",
                    Long = 46,
                    Lat = 24,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Location
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Mountain View",
                    Note = "Eastern slope with view",
                    Long = 47,
                    Lat = 25,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Name = "Forest Park",
                    Note = "Northern section of the park",
                    Long = -122,
                    Lat = 45,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Name = "Mountain Ridge",
                    Note = "Eastern ridge trail",
                    Long = -121,
                    Lat = 46,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            await dbContext.Locations.AddRangeAsync(locations);
            await dbContext.SaveChangesAsync();
            
            // Create sample cameras
            var cameras = new List<Camera>
            {
                new Camera
                {
                    Id = Guid.NewGuid(),
                    Name = "Forest Camera 1",
                    InboundEmailAddress = "forest1@trapcam.example.com",
                    LastBatteryState = 85,
                    LocationId = locations[0].Id,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Camera
                {
                    Id = Guid.NewGuid(),
                    Name = "River Camera 2",
                    InboundEmailAddress = "river2@trapcam.example.com",
                    LastBatteryState = 72,
                    LocationId = locations[1].Id,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                },
                new Camera
                {
                    Id = Guid.NewGuid(),
                    Name = "Mountain Camera 3",
                    InboundEmailAddress = "mountain3@trapcam.example.com",
                    LastBatteryState = 95,
                    LocationId = locations[2].Id,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }
            };

            await dbContext.Cameras.AddRangeAsync(cameras);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception but don't throw it to allow the application to continue
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }
    
    private static async Task SeedAnimals(AppDbContext dbContext, ILogger<AppDbContext> logger)
    {
        try
        {
            // Try multiple possible paths for the animals.txt file
            string[] possiblePaths = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "animals.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "animals.txt"),
                "/app/Data/animals.txt",
                Path.Combine(AppContext.BaseDirectory, "Data", "animals.txt")
            };
            
            string? filePath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    filePath = path;
                    break;
                }
                logger.LogInformation($"Checked path: {path} - not found");
            }
            
            // Check if any file was found
            if (filePath == null)
            {
                logger.LogWarning("Animals data file not found in any of the expected locations");
                return;
            }
            
            logger.LogInformation($"Found animals.txt at: {filePath}");
            
            logger.LogInformation($"Reading animal taxonomy data from {filePath}");
            
            // Read all lines from the file
            string[] lines = await File.ReadAllLinesAsync(filePath);
            int importedCount = 0;
            int updatedCount = 0;
            int errorCount = 0;
            
            foreach (var line in lines)
            {
                try
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                        
                    var parts = line.Split(';');
                    if (parts.Length < 7)
                    {
                        logger.LogWarning($"Invalid animal data format: {line}");
                        continue;
                    }
                    
                    // Parse the animal data
                    if (!Guid.TryParse(parts[0], out Guid animalId))
                    {
                        logger.LogWarning($"Invalid animal ID format: {parts[0]}");
                        continue;
                    }
                    
                    // Check if the animal already exists
                    var existingAnimal = await dbContext.Animals.FindAsync(animalId);
                    
                    if (existingAnimal != null)
                    {
                        // Update existing animal
                        existingAnimal.Class = parts[1];
                        existingAnimal.Order = parts[2];
                        existingAnimal.Family = parts[3];
                        existingAnimal.Genus = !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null;
                        existingAnimal.SpeciesName = !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null;
                        existingAnimal.CommonName = parts[6];
                        existingAnimal.Updated = DateTime.UtcNow;
                        
                        updatedCount++;
                    }
                    else
                    {
                        // Add new animal
                        var animal = new Animal
                        {
                            Id = animalId,
                            Class = parts[1],
                            Order = parts[2],
                            Family = parts[3],
                            Genus = !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null,
                            SpeciesName = !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null,
                            CommonName = parts[6],
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow
                        };
                        
                        await dbContext.Animals.AddAsync(animal);
                        importedCount++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing animal data line: {line}");
                    errorCount++;
                }
            }
            
            // Save all changes to the database
            await dbContext.SaveChangesAsync();
            
            logger.LogInformation($"Animal taxonomy import completed. Added: {importedCount}, Updated: {updatedCount}, Errors: {errorCount}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding animal data");
        }
    }
}
