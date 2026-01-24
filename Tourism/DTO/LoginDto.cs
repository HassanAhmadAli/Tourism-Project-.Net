using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO;

public record LoginDto(
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    string? TwoFactorCode,
    string? TwoFactorRecoveryCode
);