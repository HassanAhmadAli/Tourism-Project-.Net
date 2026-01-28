using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tourism.Data;
using Tourism.Models;

namespace Tourism.Data;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "User_email_key").IsUnique();
        });

        var inquiry = builder.Entity<InquiryModel>();
        {
            inquiry.Property(i => i.InquiryType).HasConversion<string>();
            inquiry.Property(i => i.Status).HasConversion<string>();
            inquiry
                .HasMany(i => i.Notes)
                .WithOne(n => n.Inquiry)
                .HasForeignKey(n => n.InquiryId)
                .OnDelete(DeleteBehavior.Cascade);
            inquiry
                .HasMany(i => i.Attachments)
                .WithOne(a => a.Inquiry)
                .HasForeignKey(a => a.InquiryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        var inquiryNote = builder.Entity<InquiryNoteModel>();
        {
            inquiryNote.Property(n => n.NoteType).HasDefaultValue("General");
            inquiryNote
                .HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        var inquiryAttachment = builder.Entity<InquiryAttachmentModel>();
        {
            inquiryAttachment
                .HasOne(a => a.Uploader)
                .WithMany()
                .HasForeignKey(a => a.UploaderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        OnModelCreatingPartial(builder);
    }

    public DbSet<EventMediaModel> EventMedia { get; set; }
    public DbSet<EventModel> Events { get; set; }
    public DbSet<LocationModel> Locations { get; set; }
    public DbSet<InquiryModel> InquiryModel { get; set; } = default!;
    public DbSet<InquiryNoteModel> InquiryNotes { get; set; } = default!;
    public DbSet<InquiryAttachmentModel> InquiryAttachments { get; set; } = default!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
