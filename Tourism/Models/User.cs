using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Tourism.Models;

public class User : IdentityUser
{
    [Required] [StringLength(100)] public string FullName { get; set; } = null!;
    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
}