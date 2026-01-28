using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tourism.Data;

namespace Tourism.Models;

public class InquiryNoteModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int InquiryId { get; set; }

    [ForeignKey("InquiryId")]
    public InquiryModel Inquiry { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = null!;

    [Required]
    public string AuthorId { get; set; } = null!;

    [ForeignKey("AuthorId")]
    public User Author { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(50)]
    public string? NoteType { get; set; } = "General";

    public bool IsInternal { get; set; } = false;
}
