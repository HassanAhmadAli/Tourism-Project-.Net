using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.DTO.Inquiry;

namespace Tourism.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InquiryController(ApplicationDbContext context) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult> GetInquiries([FromQuery] InquiryQueryDto query)
    {
        var queryable = context
            .InquiryModel.Include(i => i.User)
            .Include(i => i.Notes)
            .Include(i => i.Attachments)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.UserId))
        {
            queryable = queryable.Where(i => i.UserId == query.UserId);
        }
        if (query.InquiryType.HasValue)
            queryable = queryable.Where(i => i.InquiryType == query.InquiryType.Value);
        if (query.Status.HasValue)
            queryable = queryable.Where(i => i.Status == query.Status.Value);
        if (!string.IsNullOrWhiteSpace(query.Priority))
            queryable = queryable.Where(i => i.Priority == query.Priority);
        if (!string.IsNullOrWhiteSpace(query.Destination))
            queryable = queryable.Where(i =>
                i.Destination != null && i.Destination.Contains(query.Destination)
            );
        if (query.StartDate.HasValue)
            queryable = queryable.Where(i => i.SubmissionDate >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            queryable = queryable.Where(i => i.SubmissionDate <= query.EndDate.Value);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            queryable = queryable.Where(i =>
                i.CompanyName != null && i.CompanyName.Contains(query.SearchTerm)
                || i.Description != null && i.Description.Contains(query.SearchTerm)
                || i.Destination != null && i.Destination.Contains(query.SearchTerm)
            );
        }
        queryable = query.SortBy?.ToLower() switch
        {
            "submissiondate" => query.SortDescending
                ? queryable.OrderByDescending(i => i.SubmissionDate)
                : queryable.OrderBy(i => i.SubmissionDate),
            "priority" => query.SortDescending
                ? queryable.OrderByDescending(i => i.Priority)
                : queryable.OrderBy(i => i.Priority),
            "status" => query.SortDescending
                ? queryable.OrderByDescending(i => i.Status)
                : queryable.OrderBy(i => i.Status),
            _ => queryable.OrderByDescending(i => i.SubmissionDate),
        };
        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => InquiryResponseDto.fromModel(i))
            .ToListAsync();
        return Ok(
            new
            {
                items,
                meta = new
                {
                    totalCount,
                    query.PageNumber,
                    query.PageSize,
                    TotalPages = Math.Ceiling(totalCount / (double)query.PageSize),
                },
            }
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetInquiry(int id)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var inquiry = await context
            .InquiryModel.Include(i => i.User)
            .Include(i => i.Notes)
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inquiry == null)
        {
            return NotFound(new { Message = "Inquiry not found" });
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && inquiry.UserId != UserId)
        {
            return Forbid();
        }
        return Ok(InquiryResponseDto.fromModel(inquiry));
    }

    [HttpPost]
    public async Task<ActionResult> CreateInquiry(CreateInquiryDto createDto)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var Priority = createDto.Priority ?? "Medium";
        {
            switch (createDto.InquiryType)
            {
                case InquiryType.None:
                case InquiryType.Hajj:
                    break;
                case InquiryType.Corporate:
                case InquiryType.Group:
                case InquiryType.Family:
                    if (createDto.NumberOfPeople < 2)
                    {
                        return BadRequest(
                            new { Message = "the number of people in the trip must be 2 at least" }
                        );
                    }
                    break;
            }
        }
        var inquiry = new InquiryModel
        {
            CompanyName = createDto.CompanyName,
            NumberOfPeople = createDto.NumberOfPeople,
            UserId = UserId,
            InquiryType = createDto.InquiryType,
            SubmissionDate = DateTime.UtcNow,
            Status = InquiryStatus.Pending,
            Description = createDto.Description,
            PreferredTravelDate = createDto.PreferredTravelDate,
            Destination = createDto.Destination,
            Budget = createDto.Budget,
            Priority = Priority,
            LastUpdated = DateTime.UtcNow,
        };
        await context.InquiryModel.AddAsync(inquiry);
        await context.SaveChangesAsync();
        var savedInquiry = (
            await context
                .InquiryModel.Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == inquiry.Id)
        )!;
        return CreatedAtAction(
            nameof(GetInquiry),
            new { id = inquiry.Id },
            InquiryResponseDto.fromModel(savedInquiry)
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InquiryResponseDto>> UpdateInquiry(
        int id,
        UpdateInquiryDto updateDto
    )
    {
        var inquiry = await context.InquiryModel.FindAsync(id);
        if (inquiry == null)
        {
            return NotFound(new { Message = "Inquiry not found" });
        }
        if (updateDto.CompanyName != null)
            inquiry.CompanyName = updateDto.CompanyName;
        if (updateDto.NumberOfPeople.HasValue)
            inquiry.NumberOfPeople = updateDto.NumberOfPeople.Value;
        if (updateDto.InquiryType.HasValue)
            inquiry.InquiryType = updateDto.InquiryType.Value;
        if (updateDto.Status.HasValue)
            inquiry.Status = updateDto.Status.Value;
        if (updateDto.Description != null)
            inquiry.Description = updateDto.Description;
        if (updateDto.PreferredTravelDate.HasValue)
            inquiry.PreferredTravelDate = updateDto.PreferredTravelDate.Value;
        if (updateDto.EstimatedCompletionDate.HasValue)
            inquiry.EstimatedCompletionDate = updateDto.EstimatedCompletionDate.Value;
        if (updateDto.Destination != null)
            inquiry.Destination = updateDto.Destination;
        if (updateDto.Budget.HasValue)
            inquiry.Budget = updateDto.Budget.Value;
        if (updateDto.Priority != null)
            inquiry.Priority = updateDto.Priority;
        inquiry.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync();
        var updatedInquiry = (
            await context
                .InquiryModel.Include(i => i.User)
                .Include(i => i.Notes)
                .Include(i => i.Attachments)
                .FirstOrDefaultAsync(i => i.Id == id)
        )!;
        return Ok(InquiryResponseDto.fromModel(updatedInquiry));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteInquiry(int id)
    {
        var inquiry = await context.InquiryModel.FindAsync(id);
        if (inquiry == null)
        {
            return NotFound(new { Message = "Inquiry not found" });
        }
        context.InquiryModel.Remove(inquiry);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<InquiryResponseDto>> UpdateStatus(
        int id,
        [FromBody] InquiryStatus status
    )
    {
        var inquiry = await context.InquiryModel.FindAsync(id);
        if (inquiry == null)
        {
            return NotFound(new { Message = "Inquiry not found" });
        }
        inquiry.Status = status;
        inquiry.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Ok(new { Message = "Status updated successfully", status });
    }

    [HttpGet("mine")]
    public async Task<ActionResult> GetUserInquiries()
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var inquiries = await context
            .InquiryModel.Include(i => i.User)
            .Include(i => i.Notes)
            .Include(i => i.Attachments)
            .Where(i => i.UserId == UserId)
            .OrderByDescending(i => i.SubmissionDate)
            .Select(i => InquiryResponseDto.fromModel(i))
            .ToListAsync();
        return Ok(inquiries);
    }
}
