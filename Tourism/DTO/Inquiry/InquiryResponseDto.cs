using System.ComponentModel.DataAnnotations;
using Tourism.Data;

namespace Tourism.DTO.Inquiry;

public record InquiryResponseDto(
    int Id,
    string? CompanyName,
    int NumberOfPeople,
    string UserId,
    InquiryType InquiryType,
    InquiryStatus Status,
    DateTime SubmissionDate,
    string? Description,
    DateTime? PreferredTravelDate,
    DateTime? EstimatedCompletionDate,
    string? Destination,
    decimal? Budget,
    string? Priority,
    DateTime LastUpdated,
    string? UserFullName,
    string? UserEmail,
    int NoteCount,
    int AttachmentCount
)
{
    public static InquiryResponseDto fromModel(InquiryModel i)
    {
        return new InquiryResponseDto(
            i.Id,
            i.CompanyName,
            i.NumberOfPeople,
            i.UserId,
            i.InquiryType,
            i.Status,
            i.SubmissionDate,
            i.Description,
            i.PreferredTravelDate,
            i.EstimatedCompletionDate,
            i.Destination,
            i.Budget,
            i.Priority,
            i.LastUpdated,
            i.User.FullName,
            i.User.Email,
            i.Notes != null ? i.Notes.Count : 0,
            i.Attachments != null ? i.Attachments.Count : 0
        );
    }
}
