using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.Dto;
using Tourism.DTO;
using Tourism.DTO.Event;
using Tourism.DTO.Location;
using Tourism.Models;

namespace Tourism.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController(
    ApplicationDbContext context,
    IWebHostEnvironment env,
    ILogger<EventsController> logger) : ControllerBase
{

    [HttpPost("location-id/{locationId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent(int locationId, [FromBody] CreateEventDto createEventDto)
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
            Location = location
        };
        await context.Events.AddAsync(anEvent);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEventById), new { id = anEvent.Id }, new EventDto(anEvent));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllEvents()
    {
        var events = await context.Events
            .Include(e => e.Media)
            .Include(e => e.Location)
            .AsNoTracking()
            .Select(e => new EventDto(e))
            .ToListAsync();
        return Ok(events);
    }

    [HttpGet("location-id/{locationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllEventsByLocation(int locationId)
    {
        var events = await context.Events
             .Where(e => e.LocationId == locationId)
            .Include(e => e.Media)
            .Include(e => e.Location)
            .AsNoTracking()
            .Select(e => new EventDto(e))
            .ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetEventById(int id)
    {
        var anEvent = await context.Events
            .Include(e => e.Media)
            .Include(e => e.Location)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (anEvent == null)
        {
            logger.LogWarning("GetEventById: Event with ID {EventId} not found.", id);
            return NotFound();
        }
        return Ok(new EventDto(anEvent));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateEventDto)
    {
        var anEvent = await context.Events.FindAsync(id);
        if (anEvent == null)
        {
            logger.LogWarning("UpdateEvent: Event with ID {EventId} not found.", id);
            return NotFound();
        }
        if (!string.IsNullOrWhiteSpace(updateEventDto.Name))
        { anEvent.Name = updateEventDto.Name; }
        if (!string.IsNullOrWhiteSpace(updateEventDto.Description))
        { anEvent.Description = updateEventDto.Description; }
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
        if (anEvent == null) return NotFound("Event not found.");
        if (file.Length == 0) return BadRequest("No file uploaded.");
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
            MediaType = file.ContentType.StartsWith("image") ? "Image" : "Video",
            EventId = eventId
        };
        await context.EventMedia.AddAsync(anEventMedia);
        await context.SaveChangesAsync();
        var eventMediaDto = new EventMediaDto(anEventMedia);
        return Ok(eventMediaDto);
    }
}