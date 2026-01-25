using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO.Auth;

public record UserUpdateDto(
    [EmailAddress] string? NewEmail,
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string? OldPassword,
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    string? NewPassword
);