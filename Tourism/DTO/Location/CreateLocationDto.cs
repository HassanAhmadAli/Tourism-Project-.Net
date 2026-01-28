using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO.Location;

public class CreateLocationDto
{
    [Required]
    [StringLength(500)]
    public string NameEnglish { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string NameArabic { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string AddressEnglish { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string AddressArabic { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string CityEnglish { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string CityArabic { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string CountryEnglish { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string CountryArabic { get; set; } = null!;
}
