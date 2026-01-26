using System.ComponentModel.DataAnnotations;

namespace Tourism.DTO.Location;

public class UpdateLocationDto
{
    [StringLength(500)]
    public string? NameEnglish { get; set; }

    [StringLength(500)]
    public string? NameArabic { get; set; }

    [StringLength(500)]
    public string? AddressEnglish { get; set; }

    [StringLength(500)]
    public string? AddressArabic { get; set; }

    [StringLength(100)]
    public string? CityEnglish { get; set; }

    [StringLength(100)]
    public string? CityArabic { get; set; }

    [StringLength(100)]
    public string? CountryEnglish { get; set; }

    [StringLength(100)]
    public string? CountryArabic { get; set; }
}

