using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
        builder.Entity<InquiryModel>()
            .Property(i => i.InquiryType)
            .HasConversion<string>();
        OnModelCreatingPartial(builder);
    }

    public IdentityDbContext<User> users { get; set; } = null!;
    public DbSet<EventMediaModel> EventMedia { get; set; }
    public DbSet<EventModel> Events { get; set; }
    public DbSet<LocationModel> Locations { get; set; }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}