using System.ComponentModel.DataAnnotations;


namespace Tourism.Data.Models;

public class HajjTripInquiryModel
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Nationality is required.")]
    [StringLength(100)]
    public string Nationality { get; set; } = null!;

    [Required(ErrorMessage = "Company Name is required.")]
    [StringLength(200)]
    public string CompanyName { get; set; } = null!;

    [Required(ErrorMessage = "Number of people is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
    public int NumberOfPeople { get; set; }

    [Required(ErrorMessage = "A contact number is required.")]
    [Phone]
    [StringLength(20)]
    public string ContactNumber { get; set; } = null!;

    [Required(ErrorMessage = "An email address is required.")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;
}