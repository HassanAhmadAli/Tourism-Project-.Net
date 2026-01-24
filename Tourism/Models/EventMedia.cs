using System.ComponentModel.DataAnnotations;

namespace Tourism.Models;

public class EventMedia
{
    [Key] public int Id { get; set; }

    [Required] public string FilePath { get; set; } = null!;

    [Required] public string MediaType { get; set; } = null!;

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public int EventId { get; set; }

    public Event Event { get; set; } = null!;
}