using Microsoft.AspNetCore.Mvc;
using TrapCam.Backend.Entities;
using TrapCam.Backend.Services;

namespace TrapCam.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IAnimalService _animalService;
    private readonly ILogger<AnimalsController> _logger;

    public AnimalsController(IAnimalService animalService, ILogger<AnimalsController> logger)
    {
        _animalService = animalService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAnimals()
    {
        var animals = await _animalService.GetAllAnimalsAsync();
        return Ok(animals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimal(Guid id)
    {
        var animal = await _animalService.GetAnimalByIdAsync(id);
        if (animal == null)
        {
            return NotFound();
        }

        return Ok(animal);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnimal([FromBody] Animal animal)
    {
        if (animal == null)
        {
            return BadRequest();
        }

        var createdAnimal = await _animalService.CreateAnimalAsync(animal);
        return CreatedAtAction(nameof(GetAnimal), new { id = createdAnimal.Id }, createdAnimal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAnimal(Guid id, [FromBody] Animal animal)
    {
        if (animal == null)
        {
            return BadRequest();
        }

        var updatedAnimal = await _animalService.UpdateAnimalAsync(id, animal);
        if (updatedAnimal == null)
        {
            return NotFound();
        }

        return Ok(updatedAnimal);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnimal(Guid id)
    {
        var result = await _animalService.DeleteAnimalAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportAnimals([FromBody] string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return BadRequest("No data provided for import");
        }

        try
        {
            var importedCount = await _animalService.ImportAnimalsDataAsync(lines);
            return Ok(new { Message = $"Successfully imported {importedCount} animals" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing animal data");
            return StatusCode(500, "An error occurred while importing animal data");
        }
    }
}
