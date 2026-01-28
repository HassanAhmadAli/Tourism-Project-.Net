using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tourism.Models;

namespace Tourism.Data;

public enum InquiryType
{
    None = 0,
    Hajj = 1,
    Corporate = 2,
    Group = 3,
    Individual = 4,
    Family = 5,
}

public enum InquiryStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
}

public class InquiryModel
{
    [Key]
    public int Id { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Number of people must be between 1 and 1000.")]
    public int NumberOfPeople { get; set; }


    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    public InquiryType InquiryType { get; set; } = InquiryType.None;

    [Required]
    public DateTime SubmissionDate { get; set; }

    [Required]
    public InquiryStatus Status { get; set; } = InquiryStatus.Pending;

    [StringLength(500)]
    public string? Description { get; set; }
    public DateTime? PreferredTravelDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }

    [StringLength(100)]
    public string? Destination { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value.")]
    public decimal? Budget { get; set; }

    [StringLength(50)]
    public string? Priority { get; set; } = "Medium";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public virtual ICollection<InquiryNoteModel>? Notes { get; set; }
    public virtual ICollection<InquiryAttachmentModel>? Attachments { get; set; }

    public void MarkAsInProgress()
    {
        Status = InquiryStatus.InProgress;
        LastUpdated = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = InquiryStatus.Completed;
        EstimatedCompletionDate = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;
    }

    public void MarkAsCancelled(string reason)
    {
        Status = InquiryStatus.Cancelled;
        Description = $"{Description}\nCancelled: {reason}";
        LastUpdated = DateTime.UtcNow;
    }

    public bool IsOverdue()
    {
        return Status == InquiryStatus.InProgress
            && EstimatedCompletionDate.HasValue
            && EstimatedCompletionDate < DateTime.UtcNow;
    }
}
