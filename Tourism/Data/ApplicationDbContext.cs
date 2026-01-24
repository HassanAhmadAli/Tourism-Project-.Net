using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tourism.Data.Models;
using Tourism.Models;

namespace Tourism.Data;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User>(options)
{
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseNpgsql("Name=ConnectionStrings:PgConnection");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail , "User_email_key").IsUnique();
        });
        OnModelCreatingPartial(builder);
    }

    public DbSet<User> UserAccounts { get; set; }

    public DbSet<HajjTripInquiry> HajjTripInquiries { get; set; }
    public IdentityDbContext<User> users { get; set; }
    public DbSet<EventMedia> EventMedia { get; set; }
    public DbSet<Event> Events { get; set; }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}