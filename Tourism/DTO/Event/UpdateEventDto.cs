using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO.Event;

public record UpdateEventDto(
    [StringLength(100)] string? Name,
    [StringLength(1000)] string? Description,
    DateTime? EventDate,
    int? locationId
);
