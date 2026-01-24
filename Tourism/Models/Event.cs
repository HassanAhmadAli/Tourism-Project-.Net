using System.ComponentModel.DataAnnotations;

namespace Tourism.Models;

public class Event
{
    [Key] public int Id { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(1000)] public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [StringLength(200)] public string? Location { get; set; }

    public ICollection<EventMedia> Media { get; set; } = new List<EventMedia>();
}