using TrapCam.Backend.Entities;

namespace TrapCam.Backend.Services;

public interface IAnimalService
{
    Task<IEnumerable<Animal>> GetAllAnimalsAsync();
    Task<Animal?> GetAnimalByIdAsync(Guid id);
    Task<Animal> CreateAnimalAsync(Animal animal);
    Task<Animal?> UpdateAnimalAsync(Guid id, Animal animal);
    Task<bool> DeleteAnimalAsync(Guid id);
    Task<int> ImportAnimalsDataAsync(string[] lines);
}
