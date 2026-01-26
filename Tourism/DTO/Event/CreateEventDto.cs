using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record CreateEventDto(
    [Required][StringLength(100)] string Name,
    [Required][StringLength(1000)] string Description,
    [Required] DateTime EventDate
 );