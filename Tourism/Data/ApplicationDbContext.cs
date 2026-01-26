using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tourism.Data.Models;
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
        OnModelCreatingPartial(builder);
    }

    public DbSet<HajjTripInquiryModel> HajjTripInquiries { get; set; }
    public IdentityDbContext<User> users { get; set; } = null!;
    public DbSet<EventMediaModel> EventMedia { get; set; }
    public DbSet<EventModel> Events { get; set; }
    public DbSet<LocationModel> Locations { get; set; }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}