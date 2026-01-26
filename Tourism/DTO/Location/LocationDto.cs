using Tourism.Models;

namespace Tourism.DTO.Location;

public class LocationDto
{
    public LocationDto() { }
    public LocationDto(LocationModel location)
    {
        this.Id = location.Id;
        this.NameEnglish = location.NameEnglish;
        this.NameArabic = location.NameArabic;
        this.AddressEnglish = location.AddressEnglish;
        this.AddressArabic = location.AddressArabic;
        this.CityEnglish = location.CityEnglish;
        this.CityArabic = location.CityArabic;
        this.CountryEnglish = location.CountryEnglish;
        this.CountryArabic = location.CountryArabic;
    }
    public int Id { get; set; }
    public string NameEnglish { get; set; } = null!;
    public string NameArabic { get; set; } = null!;
    public string AddressEnglish { get; set; } = null!;
    public string AddressArabic { get; set; } = null!;
    public string CityEnglish { get; set; } = null!;
    public string CityArabic { get; set; } = null!;
    public string CountryEnglish { get; set; } = null!;
    public string CountryArabic { get; set; } = null!;
}
