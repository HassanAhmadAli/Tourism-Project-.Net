using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record UserRegisterDto(
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(
        100,
        MinimumLength = 4,
        ErrorMessage = "Full Name must be between 4 and 100 characters."
    )]
        string FullName,
    [Required(ErrorMessage = "Email is required.")] [EmailAddress] string Email,
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        string Password,
    [Required(ErrorMessage = "PhoneNumber is required.")]
    [MinLength(4, ErrorMessage = "PhoneNumber must be at least 4 characters long.")]
        string PhoneNumber,
    [Required(ErrorMessage = "Nationality is required.")] [StringLength(100)] string Nationality
);
