using Tourism.Dto;
using Tourism.DTO.Location;
using Tourism.Models;


namespace Tourism.DTO;

public class EventDto
{
    public EventDto() { }
    public EventDto(EventModel anEvent)
    {
        this.Id = anEvent.Id;
        this.Name = anEvent.Name;
        this.Description = anEvent.Description;
        this.EventDate = anEvent.EventDate;
        this.LocationId = anEvent.LocationId;
        this.Location = new LocationDto(anEvent.Location);
        if (anEvent.Media != null && anEvent.Media.Any())
        {
            this.Media = anEvent.Media.Select(m => new EventMediaDto(m)).ToList();
        }
    }
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime EventDate { get; set; }
    public int? LocationId { get; set; }

    public LocationDto Location { get; set; } = null!;

    public ICollection<EventMediaDto> Media { get; set; } = new List<EventMediaDto>();
}