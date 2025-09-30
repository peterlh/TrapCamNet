using Microsoft.EntityFrameworkCore;
using TrapCam.Backend.Data;
using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Services;

public class AnimalService : IAnimalService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AnimalService> _logger;

    public AnimalService(AppDbContext dbContext, ILogger<AnimalService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Animal>> GetAllAnimalsAsync()
    {
        return await _dbContext.Animals.ToListAsync();
    }

    public async Task<Animal?> GetAnimalByIdAsync(Guid id)
    {
        return await _dbContext.Animals.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Animal> CreateAnimalAsync(Animal animal)
    {
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();
        return animal;
    }

    public async Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal)
    {
        var existingAnimal = await _dbContext.Animals.FindAsync(id);
        if (existingAnimal == null)
        {
            return null;
        }

        existingAnimal.Class = animal.Class;
        existingAnimal.Order = animal.Order;
        existingAnimal.Family = animal.Family;
        existingAnimal.Genus = animal.Genus;
        existingAnimal.SpeciesName = animal.SpeciesName;
        existingAnimal.CommonName = animal.CommonName;

        await _dbContext.SaveChangesAsync();
        return existingAnimal;
    }

    public async Task<bool> DeleteAnimalAsync(Guid id)
    {
        var animal = await _dbContext.Animals.FindAsync(id);
        if (animal == null)
        {
            return false;
        }

        _dbContext.Animals.Remove(animal);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> ImportAnimalsDataAsync(string[] lines)
    {
        int importedCount = 0;

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 7)
            {
                _logger.LogWarning("Skipping line with insufficient data: {Line}", line);
                continue;
            }

            try
            {
                var animalId = Guid.Parse(parts[0]);
                
                // Check if animal with this ID already exists
                var existingAnimal = await _dbContext.Animals.FindAsync(animalId);
                if (existingAnimal != null)
                {
                    _logger.LogInformation("Animal with ID {AnimalId} already exists, updating", animalId);
                    
                    existingAnimal.Class = parts[1];
                    existingAnimal.Order = parts[2];
                    existingAnimal.Family = parts[3];
                    existingAnimal.Genus = parts[4];
                    existingAnimal.SpeciesName = parts[5];
                    existingAnimal.CommonName = parts[6];
                }
                else
                {
                    var animal = new Animal
                    {
                        Id = animalId,
                        Class = parts[1],
                        Order = parts[2],
                        Family = parts[3],
                        Genus = string.IsNullOrEmpty(parts[4]) ? null : parts[4],
                        SpeciesName = string.IsNullOrEmpty(parts[5]) ? null : parts[5],
                        CommonName = parts[6]
                    };

                    _dbContext.Animals.Add(animal);
                }

                importedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing animal data from line: {Line}", line);
            }
        }

        await _dbContext.SaveChangesAsync();
        return importedCount;
    }
}
