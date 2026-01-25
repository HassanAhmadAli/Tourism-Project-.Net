using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.DTO;
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
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        var newEvent = new Event
        {
            Name = createEventDto.Name,
            Description = createEventDto.Description,
            EventDate = createEventDto.EventDate,
            Location = createEventDto.Location
        };
        await context.Events.AddAsync(newEvent);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
    {
        var events = await context.Events
            .Include(e => e.Media)
            .AsNoTracking()
            .ToListAsync();
        return Ok(events);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Event>> GetEventById(int id)
    {
        var anEvent = await context.Events
            .Include(e => e.Media)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (anEvent == null)
        {
            logger.LogWarning("GetEventById: Event with ID {EventId} not found.", id);
            return NotFound();
        }

        return Ok(anEvent);
    }

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

        anEvent.Name = updateEventDto.Name;
        anEvent.Description = updateEventDto.Description;
        anEvent.EventDate = updateEventDto.EventDate;
        anEvent.Location = updateEventDto.Location;
        context.Events.Update(anEvent);
        await context.SaveChangesAsync();
        return NoContent();
    }


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

        var mediaRecord = new EventMedia
        {
            FilePath = $"/Uploads/Events/{uniqueFileName}",
            MediaType = file.ContentType.StartsWith("image") ? "Image" : "Video",
            EventId = eventId
        };
        await context.EventMedia.AddAsync(mediaRecord);
        await context.SaveChangesAsync();

        return Ok(new { FilePath = mediaRecord.FilePath });
    }
}