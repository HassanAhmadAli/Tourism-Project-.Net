using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tourism.Data;

namespace Tourism.Models;

public class InquiryAttachmentModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int InquiryId { get; set; }

    [ForeignKey("InquiryId")]
    public InquiryModel Inquiry { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = null!;

    [StringLength(100)]
    public string? FileType { get; set; }
    public long FileSize { get; set; }

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UploaderId { get; set; } = null!;

    [ForeignKey("UploaderId")]
    public User Uploader { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;
}
