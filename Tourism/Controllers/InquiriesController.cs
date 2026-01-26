using Microsoft.AspNetCore.Mvc;
using Tourism.Data;
using Tourism.Data.Models;
using Tourism.DTO;


namespace Tourism.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InquiriesController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("hajj")]
    public async Task<IActionResult> CreateHajjInquiry([FromBody] CreateHajjTripInquiryDto inquiryDto)
    {
        var inquiryEntity = new HajjTripInquiryModel
        {
            Nationality = inquiryDto.Nationality,
            CompanyName = inquiryDto.CompanyName,
            NumberOfPeople = inquiryDto.NumberOfPeople,
            ContactNumber = inquiryDto.ContactNumber,
            Email = inquiryDto.Email
        };

        await context.HajjTripInquiries.AddAsync(inquiryEntity);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateHajjInquiry), new { id = inquiryEntity.Id }, inquiryEntity);
    }
}