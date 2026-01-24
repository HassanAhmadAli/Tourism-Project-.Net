using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record UpdateEventDto(
    [Required] [StringLength(100)] string Name,
    [StringLength(1000)] string Description,
    DateTime EventDate,
    [StringLength(200)] string Location
);