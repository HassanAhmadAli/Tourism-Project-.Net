using Tourism.Data;

namespace Tourism.DTO.Inquiry;

public record InquiryQueryDto(
    string? UserId,
    InquiryType? InquiryType,
    InquiryStatus? Status,
    string? Priority,
    string? Destination,
    DateTime? StartDate,
    DateTime? EndDate,
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = "SubmissionDate",
    bool SortDescending = true
);
