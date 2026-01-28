using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism.Models;

public class EventModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    public int? LocationId { get; set; }

    [Required]
    [ForeignKey("LocationId")]
    public LocationModel Location { get; set; } = null!;

    public ICollection<EventMediaModel> Media { get; set; } = new List<EventMediaModel>();
}
