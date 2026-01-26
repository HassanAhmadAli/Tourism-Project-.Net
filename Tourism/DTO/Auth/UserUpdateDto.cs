using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO.Auth;

public record UserUpdateDto(
    [EmailAddress] string? Email,
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string? OldPassword,
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string? NewPassword,
    [MinLength(4, ErrorMessage = "PhoneNumber must be at least 4 characters long.")]
    string? PhoneNumber,
    [StringLength( 100,  MinimumLength =4  , ErrorMessage = "Full Name must be between 4 and 100 characters.")]
    string ? FullName,
    [Required(ErrorMessage = "Nationality is required.")]
    [StringLength(100)]
    string Nationality
);