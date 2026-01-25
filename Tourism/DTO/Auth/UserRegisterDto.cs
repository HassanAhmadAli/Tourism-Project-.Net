using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record UserRegisterDto(
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength( 100,  MinimumLength =4  , ErrorMessage = "Full Name cannot exceed 100 characters.")]
    string FullName,
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    string Email,
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string Password
);