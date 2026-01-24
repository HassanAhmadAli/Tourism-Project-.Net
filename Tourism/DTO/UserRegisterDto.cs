using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record UserRegisterDto(
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    string FullName,
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    string UserName,
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    string Email,
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string Password
);