using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.DTO;
using Tourism.DTO.Location;
using Tourism.Models;
namespace Tourism.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocationsController(ApplicationDbContext context, ILogger<LocationsController> logger) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetLocations()
    {
        try
        {
            var locations = await context.Locations
                .AsNoTracking()
                .Select(l => new LocationDto(l))
                .ToListAsync();
            return Ok(locations);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching locations. The database may not be available.");
            return StatusCode(500, "An internal server error occurred. Please try again later.");

        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetLocation(int id)
    {
        var location = await context.Locations.FindAsync(id);
        if (location == null)
        {
            return NotFound();
        }
        return Ok(new LocationDto(location));
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> PostLocation(CreateLocationDto createLocationDto)
    {

        var location = new LocationModel
        {
            NameEnglish = createLocationDto.NameEnglish,
            NameArabic = createLocationDto.NameArabic,
            AddressEnglish = createLocationDto.AddressEnglish,
            AddressArabic = createLocationDto.AddressArabic,
            CityEnglish = createLocationDto.CityEnglish,
            CityArabic = createLocationDto.CityArabic,
            CountryEnglish = createLocationDto.CountryEnglish,
            CountryArabic = createLocationDto.CountryArabic
        };

        context.Locations.Add(location);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, new LocationDto(location));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLocation(int id, UpdateLocationDto updateDto)
    {
        var location = await context.Locations.FindAsync(id);

        if (location == null)
        {
            logger.LogWarning("Update failed. Location with ID: {Id} not found.", id);
            return NotFound();
        }

        if (updateDto.NameEnglish != null) location.NameEnglish = updateDto.NameEnglish;
        if (updateDto.NameArabic != null) location.NameArabic = updateDto.NameArabic;
        if (updateDto.AddressEnglish != null) location.AddressEnglish = updateDto.AddressEnglish;
        if (updateDto.AddressArabic != null) location.AddressArabic = updateDto.AddressArabic;
        if (updateDto.CityEnglish != null) location.CityEnglish = updateDto.CityEnglish;
        if (updateDto.CityArabic != null) location.CityArabic = updateDto.CityArabic;
        if (updateDto.CountryEnglish != null) location.CountryEnglish = updateDto.CountryEnglish;
        if (updateDto.CountryArabic != null) location.CountryArabic = updateDto.CountryArabic;


        context.Entry(location).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LocationExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await context.Locations.FindAsync(id);
        if (location == null)
        {
            logger.LogWarning("Delete failed. Location with ID: {Id} not found.", id);
            return NotFound();
        }

        context.Locations.Remove(location);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool LocationExists(int id)
    {
        return context.Locations.Any(e => e.Id == id);
    }
}
