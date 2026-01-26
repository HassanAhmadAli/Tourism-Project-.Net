
using Tourism.Models;

namespace Tourism.Dto;

public class EventMediaDto
{
    public EventMediaDto(EventMediaModel media)
    {
        Id = media.Id;
        FilePath = media.FilePath;
        MediaType = media.MediaType;
        UploadDate = media.UploadDate;
        EventId = media.EventId;
    }
    public int Id { get; set; }

    public string FilePath { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int EventId { get; set; }
}