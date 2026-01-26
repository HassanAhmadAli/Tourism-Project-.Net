using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tourism.Models;


namespace Tourism.Data;


public enum InquiryType
{
    None = 0,
    Hajj = 1,
    Corporate = 2,
    Group = 3
}

public class InquiryModel
{
    [Key]
    public int Id { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Number of people is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
    public int NumberOfPeople { get; set; }


    [Required(ErrorMessage = "An email address is required.")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    public InquiryType InquiryType { get; set; } = InquiryType.None;

    [Required]
    public DateTime SubmissionDate { get; set; }
}