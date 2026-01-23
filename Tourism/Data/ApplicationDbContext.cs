using Microsoft.EntityFrameworkCore;
using Tourism.Models;

namespace Tourism.Data;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:PgConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    public DbSet<User> UserAccounts { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}