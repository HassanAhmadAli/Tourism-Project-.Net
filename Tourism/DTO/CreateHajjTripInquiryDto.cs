using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record CreateHajjTripInquiryDto(
    [Required(ErrorMessage = "Nationality is required.")]
    [StringLength(100)]
    string Nationality,

    [Required(ErrorMessage = "Company Name is required.")]
    [StringLength(200)]
    string CompanyName,

    [Required(ErrorMessage = "Number of people is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
    int NumberOfPeople,

    [Required(ErrorMessage = "A contact number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20)]
    string ContactNumber,

    [Required(ErrorMessage = "An email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100)]
    string Email
);