using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.Dto;
using Tourism.DTO;
using Tourism.DTO.Event;
using Tourism.Models;

namespace Tourism.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController(
    ApplicationDbContext context,
    IWebHostEnvironment env,
    ILogger<EventsController> logger
) : ControllerBase
{
    [HttpPost("location-id/{locationId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent(
        int locationId,
        [FromBody] CreateEventDto createEventDto
    )
    {
        var location = await context.Locations.FindAsync(locationId);
        if (location == null)
        {
            return BadRequest($"Location with ID {locationId} not found.");
        }
        var anEvent = new EventModel
        {
            Name = createEventDto.Name,
            Description = createEventDto.Description,
            EventDate = createEventDto.EventDate,
            Location = location,
        };
        await context.Events.AddAsync(anEvent);
        await context.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetEventById),
            new { id = anEvent.Id },
            new EventDto(anEvent)
        );
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllEvents([FromQuery] int? locationId)
    {
        var eventsQuerable = context.Events.AsQueryable();
        if (locationId is not null)
        {
            eventsQuerable = eventsQuerable.Where(e => e.LocationId == locationId);
        }
        var events = await eventsQuerable
            .Include(e => e.Media)
            .Include(e => e.Location)
            .AsNoTracking()
            .Select(e => new EventDto(e))
            .ToListAsync();
        return Ok(events);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetEventById(int id)
    {
        var anEvent = await context
            .Events.Include(e => e.Media)
            .Include(e => e.Location)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (anEvent == null)
        {
            return NotFound();
        }
        return Ok(new EventDto(anEvent));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateEventDto)
    {
        var anEvent = await context.Events.FindAsync(id);
        if (anEvent == null)
        {
            return NotFound();
        }
        if (!string.IsNullOrWhiteSpace(updateEventDto.Name))
        {
            anEvent.Name = updateEventDto.Name;
        }
        if (!string.IsNullOrWhiteSpace(updateEventDto.Description))
        {
            anEvent.Description = updateEventDto.Description;
        }
        if (updateEventDto.EventDate is not null)
        {
            anEvent.EventDate = (DateTime)updateEventDto.EventDate;
        }
        if (updateEventDto.locationId is not null)
        {
            anEvent.LocationId = updateEventDto.locationId;
        }
        context.Events.Update(anEvent);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(300 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 300 * 1024 * 1024)]
    [HttpPost("{eventId}/media")]
    public async Task<IActionResult> UploadEventMedia(int eventId, [FromForm] IFormFile file)
    {
        var anEvent = await context.Events.FindAsync(eventId);
        if (anEvent == null)
            return NotFound(new { Message = "Event not found." });
        if (file.Length == 0)
            return BadRequest(new { Message = "No file uploaded." });
        var uploadsFolderPath = Path.Combine(env.ContentRootPath, "Uploads", "Events");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var anEventMedia = new EventMediaModel
        {
            FilePath = $"/Uploads/Events/{uniqueFileName}",
            MediaType = file.ContentType,
            EventId = eventId,
        };
        await context.EventMedia.AddAsync(anEventMedia);
        await context.SaveChangesAsync();
        var eventMediaDto = new EventMediaDto(anEventMedia);
        return Ok(eventMediaDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{eventId}/media/{mediaId}")]
    public async Task<ActionResult> deleteEventMedia(int eventId, int mediaId)
    {
        var mediaToDelete = await context.EventMedia.FindAsync(mediaId);
        if (mediaToDelete == null)
        {
            return NotFound(new { Message = $"No media found with ID {mediaId}." });
        }
        if (mediaToDelete.EventId != eventId)
        {
            return BadRequest(
                new { Message = "This media item does not belong to the specified event." }
            );
        }
        if (!string.IsNullOrWhiteSpace(mediaToDelete.FilePath))
        {
            if (System.IO.File.Exists(mediaToDelete.FilePath))
            {
                System.IO.File.Delete(mediaToDelete.FilePath);
            }
        }
        context.EventMedia.Remove(mediaToDelete);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
