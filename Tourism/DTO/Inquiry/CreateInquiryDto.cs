using System.ComponentModel.DataAnnotations;
using Tourism.Data;

namespace Tourism.DTO.Inquiry;

public record CreateInquiryDto(
    [StringLength(200)] string? CompanyName,
    [Required(ErrorMessage = "Inquiry type is required.")] InquiryType InquiryType,
    [StringLength(500)] string? Description,
    DateTime? PreferredTravelDate,
    [StringLength(100)] string? Destination,
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value.")] decimal? Budget,
    [Range(1, 1000, ErrorMessage = "Number of people must be between 1 and 1000.")]
        int NumberOfPeople = 1,
    [StringLength(50)] string? Priority = "Medium"
);
